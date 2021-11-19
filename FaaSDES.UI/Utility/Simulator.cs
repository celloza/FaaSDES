using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using t = System.Threading.Tasks;

namespace FaaSDES.UI
{
    public class Simulator
    {
        internal IEnumerable<Property> Properties { get; set; }
        public XElement ProcessXML { get; set; }
        public XNamespace NS { get; set; }
        private Simulator() { }

        public Simulator(Stream bpmnStream)
        {
            XDocument doc = XDocument.Load(bpmnStream);
            NS = @"http://www.omg.org/spec/BPMN/20100524/MODEL";
            ProcessXML = doc.Root.Element(NS + "process");
            Properties = PropertyInitializer(ProcessXML, NS);
        }

        public SimulationInstance NewProcessInstance()
        {
            var current = ProcessXML.Element(NS + "startEvent");
            var node = new SimulationNode(current.Attribute("id").Value, current.Name.LocalName);
            var nodes = BuildNodes(ProcessXML);
            var processInstance = new SimulationInstance(this);
            BuildLinkedNodes(current, ref node, nodes, processInstance);
            processInstance.Id = Guid.NewGuid().ToString();
            processInstance.StartNode = node;
            processInstance.Nodes = nodes.ToImmutableDictionary();

            return processInstance;
        }

        private IDictionary<string, SimulationNode> BuildNodes(XElement processXML)
        {
            var nodes = processXML.Elements().ToDictionary(e => e.Attribute("id").Value, e => new SimulationNode(e.Attribute("id").Value, e.Name.LocalName, e.Attribute("name") != null ? e.Attribute("name").Value : string.Empty));

            nodes.Where(e => e.Value.NodeType == "property").Select(e => e.Key).ToList().ForEach(k => nodes.Remove(k));
            nodes.Where(e => e.Value.NodeType == "textAnnotation").Select(e => e.Key).ToList().ForEach(k => nodes.Remove(k));
            nodes.Where(e => e.Value.NodeType == "association").Select(e => e.Key).ToList().ForEach(k => nodes.Remove(k));

            var scripts = processXML.Elements().Elements(NS + "script")
                .Select(s => new { id = s.Parent.Attribute("id").Value, expression = s.Value });

            foreach (var s in scripts) nodes[s.id].Expression = s.expression;

            var conditionExpressions = processXML.Elements().Elements(NS + "conditionExpression")
                .Select(c => new { id = c.Parent.Attribute("id").Value, expression = c.Value });

            foreach (var c in conditionExpressions) nodes[c.id].Expression = c.expression;

            //Quick fix for zmq example
            //TODO Proper process var/assignment to node var mapping
            var taskExpressions = processXML.Elements(NS + "task").Elements(NS + "dataInputAssociation").Elements(NS + "assignment").Elements(NS + "from")
                .Select(e => new { id = e.Parent.Parent.Parent.Attribute("id").Value, expression = e.Value });

            foreach (var e in taskExpressions) nodes[e.id].Expression = e.expression;

            
            

            return nodes;
        }

        private Func<XElement, XElement, XNamespace, IEnumerable<XElement>> NextSequences =
            (e, ProcessXML, NS) => ProcessXML.Elements(NS + "sequenceFlow")?
            .Where(s => s.Attribute("sourceRef")?.Value == e.Attribute("id").Value);

        private Func<XElement, XElement, IEnumerable<XElement>> NextElement =
            (s, ProcessXML) => ProcessXML.Elements()
            .Where(e => e.Attribute("id").Value == s.Attribute("targetRef")?.Value);

        private void BuildLinkedNodes(XElement current, ref SimulationNode node, IDictionary<string, SimulationNode> nodes, SimulationInstance processInstance)
        {
            node.SimulationInstance = processInstance;
            var seq = NextSequences(current, ProcessXML, NS);
            var next = (seq.Any() ? seq : NextElement(current, ProcessXML));
            node.NextNodes = new List<SimulationNode>();

            foreach (var n in next)
            {
                var nextNode = nodes[n.Attribute("id").Value];
                if (nextNode.PreviousNodes == null) nextNode.PreviousNodes = new List<SimulationNode>();
                if (!nextNode.PreviousNodes.Contains(node)) nextNode.PreviousNodes.Add(node);
                node.NextNodes.Add(nextNode);
                BuildLinkedNodes(n, ref nextNode, nodes, processInstance);
            }
        }

        internal string GetAssociation(string nodeId, string nodeVariableName)
        {
            var node = ProcessXML.Elements().Where(e => e.Attribute("id").Value == nodeId);
            var inputId = node.Elements(NS + "ioSpecification").Elements(NS + "dataInput")
                .Where(e => e.Attribute("name").Value == nodeVariableName).FirstOrDefault().Attribute("id").Value;
            var propertyId = node.Elements(NS + "dataInputAssociation")
                .Where(d => d.Element(NS + "targetRef").Value == inputId).Elements(NS + "sourceRef").FirstOrDefault().Value;
            var propertyName = ProcessXML.Elements(NS + "property")
                .Where(e => e.Attribute("id").Value == propertyId).Attributes("name").FirstOrDefault().Value;
            return propertyName;
        }

        private IEnumerable<Property> PropertyInitializer(XElement process, XNamespace ns)
        {
            var itemDefinitions = process.Parent.Elements(ns + "itemDefinition");
            var properties = process.Elements(ns + "property").ToList();
            var propertyList = new List<Property>();
            foreach (var property in properties)
            {
                string id = property.Attribute("id").Value;
                string name = property.Attribute("name").Value;
                string itemSubjectRef = property.Attribute("itemSubjectRef").Value;
                string structureRef = itemDefinitions
                    .Where(i => i.Attribute("id").Value == itemSubjectRef)
                    .FirstOrDefault()
                    .Attribute("structureRef")
                    .Value;
                bool isCollection = Convert.ToBoolean(itemDefinitions
                    .Where(i => i.Attribute("id").Value == itemSubjectRef)
                    .FirstOrDefault()
                    .Attribute("isCollection")
                    .Value);
                propertyList.Add(new Property(id, name, structureRef, isCollection));
            }

            return propertyList;
        }
    }

    internal class Property
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StructureRef { get; set; }
        public bool IsCollection { get; set; }

        public Property(string id, string name, string structureRef, bool isCollection)
        {
            Id = id;
            Name = name;
            StructureRef = structureRef;
            IsCollection = isCollection;
        }
    }

    public class SimulationInstance
    {
        public string Id { get; set; }

        public Simulator Simulation { get; }

        private IImmutableDictionary<string, object> inputParameters;

        public IImmutableDictionary<string, object> InputParameters
        {
            get
            {
                return inputParameters;
            }

            set
            {
                if (ValidParameters(value))
                    inputParameters = value;
                else
                    throw new Exception("Parameter type does not match process definition");
            }
        }

        public IImmutableDictionary<string, IImmutableDictionary<string, object>> OutputParameters { get; set; }

        public SimulationNode StartNode { get; internal set; }

        public IImmutableDictionary<string, SimulationNode> Nodes { get; set; }

        private IDictionary<string, INodeHandler> nodeHandlers;

        public IDictionary<string, INodeHandler> NodeHandlers
        {
            get
            {
                return nodeHandlers;
            }

            set
            {
                if (ValidHandlers(value))
                    nodeHandlers = value;
                else
                    throw new Exception("Unhandled node type");
            }
        }

        public SimulationInstance(Simulator process)
        {
            Simulation = process;
        }

        public void Start()
        {
            StartNode.Execute(StartNode, null);
        }

        public void SetDefaultHandlers()
        {
            var defaultNodeHandlers = new Dictionary<string, INodeHandler>()
            {
                { "startEvent", new DefaultStartHandler()},
                { "endEvent", new DefaultEndHandler()},
                { "intermediateThrowEvent", new DefaultIntermediateHandler()},
                { "task", new DefaultTaskHandler()},
                { "sequenceFlow", new DefaultSequenceHandler()},
                { "businessRuleTask", new DefaultBusinessRuleHandler()},
                { "exclusiveGateway", new DefaultExclusiveGatewayHandler()},
                { "inclusiveGateway", new DefaultInclusiveGatewayHandler()},
                { "parallelGateway", new DefaultParallelGatewayHandler()},
                { "scriptTask", new DefaultScriptTaskHandler()}
            };

            if (Nodes.All(t => defaultNodeHandlers.ContainsKey(t.Value.NodeType)))
            {
                nodeHandlers = new Dictionary<string, INodeHandler>();
                foreach (string n in Nodes.Values.Select(n => n.NodeType).Distinct())
                {
                    nodeHandlers.Add(n, defaultNodeHandlers[n]);
                }
            }
            else
            {
                var unknownNodeTypes = Nodes.Where(t => !defaultNodeHandlers.ContainsKey(t.Value.NodeType)).ToList();
                throw new Exception("Process contains an unknown node type");
            }
        }

        public void SetHandler(string nodeType, INodeHandler nodeHandler)
        {
            if (nodeHandlers == null)
                nodeHandlers = new Dictionary<string, INodeHandler>();

            if (nodeHandlers.ContainsKey(nodeType))
                nodeHandlers[nodeType] = nodeHandler;
            else
                nodeHandlers.Add(nodeType, nodeHandler);
        }

        private bool ValidHandlers(IDictionary<string, INodeHandler> handlers)
        {
            var nodeTypes = Nodes.Values.Select(n => n.NodeType).Distinct();
            return nodeTypes.All(t => handlers.ContainsKey(t));
        }

        private bool ValidParameters(IImmutableDictionary<string, object> parameters)
        {
            var propertyMap = Simulation.Properties.ToDictionary(p => p.Name, p => p.StructureRef);
            return parameters.All(p => p.Value.GetType().Name.ToLower() == propertyMap[p.Key].ToLower());
        }

        public void Start(IDictionary<string, object> parameters)
        {
            //TODO Get node variables not process instance var
            InputParameters = parameters.ToImmutableDictionary();
            StartNode.InputParameters = parameters.ToImmutableDictionary();
            Start();
        }
        internal void SetOutputParameters(SimulationNode node)
        {
            if (OutputParameters == null)
            {
                OutputParameters = ImmutableDictionary.Create<string, IImmutableDictionary<string, object>>();
            }

            OutputParameters.Add(node.NodeName, node.OutputParameters);
        }
    }

    public interface INodeHandler
    {
        void Execute(SimulationNode currentNode, SimulationNode previousNode);
    }

    public class SimulationNode
    {
        public string NodeName { get; set; }
        public string NodeType { get; set; }
        public SimulationInstance SimulationInstance { get; set; }
        public IImmutableDictionary<string, object> InputParameters { get; set; }
        public IImmutableDictionary<string, object> OutputParameters { get; set; }
        public INodeHandler NodeHandler { get; set; }
        public ICollection<SimulationNode> NextNodes { get; set; }
        public ICollection<SimulationNode> PreviousNodes { get; set; }
        private t.Task Task { get; set; }
        public string Expression { get; set; }

        public string Content { get; set; }

        public SimulationNode()
        {
        }

        public SimulationNode(INodeHandler nodeHandler)
        {
            NodeHandler = nodeHandler;
        }

        public SimulationNode(string name, string type)
        {
            NodeName = name;
            NodeType = type;
        }

        public SimulationNode(string name, string type, string content)
        {
            NodeName = name;
            NodeType = type;
            Content = content;
        }

        public void Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            NodeHandler = SimulationInstance.NodeHandlers[NodeType];
            if (currentNode.InputParameters == null) currentNode.InputParameters = SimulationInstance.InputParameters;
            Task = new t.Task(() => NodeHandler.Execute(currentNode, previousNode));
            Task.Start();
        }
        public void Done()
        {
            foreach (var node in NextNodes)
            {
                //to replace with variable resolution
                //for each node retrieve input parameters defined in BPMN
                //retrieve from node.OutputParameters (results of previous node)
                //retrieve missing necessary input from process variables
                node.InputParameters = OutputParameters;
                node.Execute(node, this);
            }
        }
    }

    internal class DefaultTaskHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName + " Executing Task");
            currentNode.Done();
        }
    }

    internal class DefaultStartHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName + " Executing Start");
            currentNode.Done();
        }
    }

    internal class DefaultParallelGatewayHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName);
            currentNode.Done();
        }
    }

    internal class DefaultExclusiveGatewayHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName);
            currentNode.Done();
        }
    }

    internal class DefaultBusinessRuleHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName + " Executing BusinessRule");
            currentNode.Done();
        }
    }

    internal class DefaultSequenceHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName + " Executing Sequence");
            bool result = true;
            if (currentNode.Expression != null)
            {
                Trace.WriteLine(currentNode.NodeName + " Conditional Sequence");
                Trace.WriteLine("Condition: " + currentNode.Expression);
                var globals = new Globals(currentNode.InputParameters.ToDictionary(e => e.Key, e => e.Value));
                try
                {
                    //result = CSharpScript.EvaluateAsync<bool>(processNode.Expression, globals: globals).Result;
                    //Console.WriteLine("Condition result: " + result.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (result)
            {
                currentNode.Done();
            }
        }
    }

    internal class DefaultEndHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName + " Executing End");
            currentNode.SimulationInstance.SetOutputParameters(currentNode);
            currentNode.Done();
        }
    }

    internal class DefaultIntermediateHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName + " Executing End");
            currentNode.SimulationInstance.SetOutputParameters(currentNode);
            currentNode.Done();
        }
    }

    internal class DefaultScriptTaskHandler : INodeHandler
    {
        void INodeHandler.Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Console.WriteLine(currentNode.NodeName + " Executing Script");

            if (currentNode.Expression != null)
            {
                Console.WriteLine("Script: " + currentNode.Expression);
                var globals = new Globals(currentNode.InputParameters.ToDictionary(e => e.Key, e => e.Value));
                try
                {
                    //processNode.OutputParameters =
                    //    CSharpScript.EvaluateAsync<IDictionary<string, object>>(processNode.Expression, globals: globals)
                    //    .Result.ToImmutableDictionary();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            currentNode.Done();
        }
    }

    public class Globals
    {
        public IDictionary<string, object> globals;
        public Globals(IDictionary<string, object> parameters)
        {
            globals = parameters;
        }
    }

    internal class DefaultInclusiveGatewayHandler : INodeHandler
    {
        ConcurrentDictionary<SimulationNode, ICollection<SimulationNode>> sequenceWait = new();

        void INodeHandler.Execute(SimulationNode simulationNode, SimulationNode previousNode)
        {
            Trace.WriteLine(simulationNode.NodeName);
            sequenceWait.GetOrAdd(simulationNode, new List<SimulationNode>(simulationNode.PreviousNodes));
            lock (sequenceWait[simulationNode])
            {
                sequenceWait[simulationNode].Remove(previousNode);
            }
            if (sequenceWait[simulationNode].Count == 0)
            {
                simulationNode.Done();
            }
        }
    }

}