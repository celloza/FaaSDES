using FaaSDES.Sim.Nodes;
using FaaSDES.Sim.Tokens.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FaaSDES.Sim
{
    /// <summary>
    /// The <see cref="Simulator"/> contains all the necessary state and logic to perform multiple
    /// expirements (or <see cref="Simulation"/>s.
    /// </summary>
    public class Simulator
    {
        public IEnumerable<ISimNode> Nodes { get; set; }

        public StartSimNode StartNode { get; set; }

        public Simulator(ISimTokenGenerator tokenGenerator, SimulatorSettings settings)
        {
            _tokenGenerator = tokenGenerator;
            _settings = settings;
        }

        public void BuildSimulatorFromBpmnXml(FileStream streamToFile)
        {
            if(streamToFile == null)
                throw new ArgumentNullException(nameof(streamToFile));

            XDocument doc = XDocument.Load(streamToFile);
            BpmnNamespace = @"http://www.omg.org/spec/BPMN/20100524/MODEL";

            if (doc.Root == null || doc.Root.Element(BpmnNamespace + "process") == null)
                throw new InvalidOperationException("The provided XML file does not contain a root process node.");
            else
            {
                SourceBpmn = doc.Root.Element(BpmnNamespace + "process");
                Properties = PropertyInitializer(SourceBpmn);
            }

        }

        public Simulation NewSimulationInstance()
        {
            Simulation sim = new Simulation(this);

            var start = SourceBpmn.Element(BpmnNamespace + "startEvent");
            StartNode = new StartSimNode(sim, _tokenGenerator, start.Attribute("id").Value, start.Attribute("id").Value);
            

            Nodes = BuildAndLinkNodes(SourceBpmn);
            
            //var nodes = BuildNodes(ProcessXML);
            //var processInstance = new ProcessInstance(this);
            
            //BuildLinkedNodes(current, ref node, nodes, processInstance);
            //processInstance.Id = Guid.NewGuid().ToString();
            //processInstance.StartNode = node;
            //processInstance.Nodes = nodes.ToImmutableDictionary();

            return sim;
        }

        private IEnumerable<ISimNode> BuildAndLinkNodes(XElement source)
        {
            List<SimNodeBase> nodes = new List<SimNodeBase>();
            
            var flows = source.Elements().Where(x => x.Name == BpmnNamespace + "sequenceFlow");

            var start = SourceBpmn.Element(BpmnNamespace + "startEvent");
            //Add the outgoing flow(s) on the StartEvent
            LinkSequenceFlows(StartNode, start, flows);
            nodes.Add(StartNode);

            // find all tasks
            var activities = source.Elements().Where(x => x.Name == BpmnNamespace + "task");
            foreach(var activity in activities)
            {
                ActivitySimNode node = new ActivitySimNode(activity.Attribute("id").Value,
                   activity.Name.LocalName);
                LinkSequenceFlows(node, activity, flows);
                nodes.Add(node);
            }

            // find all exclusiveGateways
            var exclusiveGateways = source.Elements().Where(x => x.Name == BpmnNamespace + "exclusiveGateway");
            foreach (var exclusiveGateway in exclusiveGateways)
            {
                GatewaySimNode node = new GatewaySimNode(exclusiveGateway.Attribute("id").Value,
                   exclusiveGateway.Name.LocalName);
                LinkSequenceFlows(node, exclusiveGateway, flows);
                nodes.Add(node);
            }

            // find all inclusiveGateways
            var inclusiveGateways = source.Elements().Where(x => x.Name == BpmnNamespace + "inclusiveGateway");
            foreach (var inclusiveGateway in inclusiveGateways)
            {
                GatewaySimNode node = new GatewaySimNode(inclusiveGateway.Attribute("id").Value,
                   inclusiveGateway.Name.LocalName);
                LinkSequenceFlows(node, inclusiveGateway, flows);
                nodes.Add(node);
            }

            // find all intermediateThrowEvents
            var intermediateThrowEvents = source.Elements().Where(x => x.Name == BpmnNamespace + "intermediateThrowEvent");
            foreach (var intermediateThrowEvent in intermediateThrowEvents)
            {
                EventSimNode node = new EventSimNode(intermediateThrowEvent.Attribute("id").Value,
                   intermediateThrowEvent.Name.LocalName);
                LinkSequenceFlows(node, intermediateThrowEvent, flows);
                nodes.Add(node);
            }

            // find all intermediateThrowEvents
            var parallelGateways = source.Elements().Where(x => x.Name == BpmnNamespace + "parallelGateway");
            foreach (var parallelGateway in parallelGateways)
            {
                GatewaySimNode node = new GatewaySimNode(parallelGateway.Attribute("id").Value,
                   parallelGateway.Name.LocalName);
                LinkSequenceFlows(node, parallelGateway, flows);
                nodes.Add(node);
            }

            // find all endEvents
            var endEvents = source.Elements().Where(x => x.Name == BpmnNamespace + "endEvent");
            foreach (var endEvent in endEvents)
            {
                EventSimNode node = new EventSimNode(endEvent.Attribute("id").Value,
                   endEvent.Name.LocalName);
                LinkSequenceFlows(node, endEvent, flows);
                nodes.Add(node);
            }

            // not supported at the moment: textAnnotation, association

            // Get all the sequence flow information first. When building nodes, we can refer back to this list to get
            // the information of the flow.
            
            foreach (var flow in flows)
            {

                var sourceNodes = nodes.Where(x => x.OutboundFlows.Any(y => y.Id == flow.Attribute("id").Value));

                if(sourceNodes.Count() != 1)
                {
                    throw new InvalidOperationException("More than one node in the BPMN XML is connected to a single SequenceFlow. The document is invalid.");
                }

                var sourceNode = sourceNodes.Single();

                var destinationNodes = nodes.Where(x => x.InboundFlows.Any(y => y.Id == flow.Attribute("id").Value));

                if (destinationNodes.Count() != 1)
                {
                    throw new InvalidOperationException("More than one node in the BPMN XML is connected to a single SequenceFlow. The document is invalid.");
                }

                var destinationNode = destinationNodes.Single();

                // find the source node
                var sourceNodeFlow = sourceNode.OutboundFlows.Single(x => x.Id == flow.Attribute("id").Value);
                var destinationNodeFlow = destinationNode.InboundFlows.Single(x => x.Id == flow.Attribute("id").Value);

                sourceNodeFlow.TargetNode = destinationNodeFlow.TargetNode;
                destinationNodeFlow.SourceNode = sourceNodeFlow.SourceNode;
            }

            return nodes;
        }

        public void LinkSequenceFlows(SimNodeBase node, XElement nodeSource, IEnumerable<XElement> flows)
        {
            foreach (var inboundFlowId in nodeSource.Elements().Where(x => x.Name.LocalName == "incoming").Select(x => x.Value))
            {
                var singleFlow = flows.Single(x => x.Attribute("id").Value == inboundFlowId);
                var newFlow = new SequenceFlow(singleFlow.Attribute("sourceRef").Value, node.Id, inboundFlowId);
                newFlow.TargetNode = node;
                ((List<SequenceFlow>)node.InboundFlows).Add(newFlow);
                // how to set sourceNode here?
            }

            foreach (var outboundFlowId in nodeSource.Elements().Where(x => x.Name.LocalName == "outgoing").Select(x => x.Value))
            {
                var singleFlow = flows.Single(x => x.Attribute("id").Value == outboundFlowId);
                var newFlow = new SequenceFlow(node.Id, singleFlow.Attribute("targetRef").Value, outboundFlowId);
                newFlow.SourceNode = node;
                ((List<SequenceFlow>)node.OutboundFlows).Add(newFlow);
                // how to set targetNode here?
            }
        }        

        private IEnumerable<BpmnProperty> PropertyInitializer(XElement source)
        {
            var itemDefinitions = source.Parent.Elements(BpmnNamespace + "itemDefinition");
            var properties = source.Elements(BpmnNamespace + "property").ToList();
            var propertyList = new List<BpmnProperty>();
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
                propertyList.Add(new BpmnProperty(id, name, structureRef, isCollection));
            }

            return propertyList;
        }

        private XElement SourceBpmn;
        private XNamespace BpmnNamespace;
        internal IEnumerable<BpmnProperty> Properties;
        private readonly ISimTokenGenerator _tokenGenerator;
        private readonly SimulatorSettings _settings;
    }

    internal class BpmnProperty
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StructureRef { get; set; }
        public bool IsCollection { get; set; }

        public BpmnProperty(string id, string name, string structureRef, bool isCollection)
        {
            Id = id;
            Name = name;
            StructureRef = structureRef;
            IsCollection = isCollection;
        }
    }
}
