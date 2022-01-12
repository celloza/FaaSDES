using FaaSDES.Sim.Nodes;
using FaaSDES.Sim.Tokens.Generation;
using System.Xml.Linq;

namespace FaaSDES.Sim
{
    /// <summary>
    /// The <see cref="Simulator"/> contains all the necessary state and logic to perform multiple
    /// expirements (or <see cref="Simulation"/>s.
    /// </summary>
    public class Simulator
    {
        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="Simulator"/> instance with the provided 
        /// <see cref="ISimTokenGenerator"/> and <see cref="SimulationSettings"/>.
        /// </summary>
        /// <param name="streamToFile">A <see cref="Stream"/> object for the source BPMN model.
        public static Simulator FromBpmnXML(FileStream streamToFile)
        {
            ArgumentNullException.ThrowIfNull(nameof(streamToFile));

            Simulator simulator = new();

            simulator.BpmnNamespace = @"http://www.omg.org/spec/BPMN/20100524/MODEL";

            XDocument doc = XDocument.Load(streamToFile);

            if (doc.Root == null || doc.Root.Element(simulator.BpmnNamespace + "process") == null)
                throw new InvalidOperationException("The provided XML file does not contain a root process node.");
            else
            {
                simulator.SourceBpmn = doc.Root.Element(simulator.BpmnNamespace + "process");
                simulator.Properties = simulator.PropertyInitializer(simulator.SourceBpmn);
            }

            return simulator;
        }

        /// <summary>
        /// Creates a new <see cref="Simulator"/> instance with the provided 
        /// <see cref="ISimTokenGenerator"/> and <see cref="SimulationSettings"/>.
        /// </summary>
        /// <param name="xmlContent">A string containing the source BPMN model as BPMN XML.
        public static Simulator FromBpmnXML(string xmlContent)
        {
            ArgumentNullException.ThrowIfNull(nameof(xmlContent));

            Simulator simulator = new();

            simulator.BpmnNamespace = @"http://www.omg.org/spec/BPMN/20100524/MODEL";

            XDocument doc = XDocument.Parse(xmlContent);

            if (doc.Root == null || doc.Root.Element(simulator.BpmnNamespace + "process") == null)
                throw new InvalidOperationException("The provided XML file does not contain a root process node.");
            else
            {
                simulator.SourceBpmn = doc.Root.Element(simulator.BpmnNamespace + "process");
                simulator.Properties = simulator.PropertyInitializer(simulator.SourceBpmn);
            }

            return simulator;
        }

        /// <summary>
        /// Creates a new <see cref="Simulation"/> instance based on this <see cref="Simulator"/>.
        /// </summary>
        /// <returns></returns>
        public Simulation NewSimulationInstance(SimulationSettings settings, ISimTokenGenerator tokenGenerator)
        {
            Simulation sim = new(this, tokenGenerator, settings);

            sim.Nodes = BuildAndLinkNodes(SourceBpmn, sim, out StartSimNode startNode);

            sim.StartNode = startNode;
            sim.StartNode.EnableStats();
            return sim;
        }

        #endregion

        #region Constructors

        private Simulator() { }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parses the provided BPMN XML contents into POCOs.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sim"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IEnumerable<ISimNode> BuildAndLinkNodes(XElement source, Simulation sim, out StartSimNode startNode)
        {
            List<SimNodeBase> nodes = new();

            var flows = source.Elements().Where(x => x.Name == BpmnNamespace + "sequenceFlow");

            var start = SourceBpmn.Element(BpmnNamespace + "startEvent");
            startNode = new(sim, start.Attribute("id").Value, start.Attribute("id").Value);

            //Add the outgoing flow(s) on the StartEvent
            LinkSequenceFlows(startNode, start, flows);
            nodes.Add(startNode);

            // find all tasks
            var activities = source.Elements().Where(x => x.Name.LocalName.EndsWith("task", StringComparison.OrdinalIgnoreCase));
            foreach (var activity in activities)
            {
                string nodeName = string.Empty;

                if (activity.Attribute("name") != null)
                    nodeName = activity.Attribute("name").Value;

                ActivitySimNode node = new(sim, activity.Attribute("id").Value,
                   nodeName);                

                switch (activity.Name.LocalName)
                {
                    case "task":
                        node.Type = ActivitySimNodeType.Undefined;
                        break;
                    case "manualTask":
                        node.Type = ActivitySimNodeType.Manual;
                        break;
                    case "sendTask":
                        node.Type = ActivitySimNodeType.Send;
                        break;
                    case "receiveTask":
                        node.Type = ActivitySimNodeType.Receive;
                        break;
                    case "scriptTask":
                        node.Type = ActivitySimNodeType.Script;
                        break;
                    case "receiveInstantiatedTask":
                        node.Type = ActivitySimNodeType.ReceiveInstantiated;
                        break;
                    case "serviceTask":
                        node.Type = ActivitySimNodeType.Service;
                        break;
                    case "businessRuleTask":
                        node.Type = ActivitySimNodeType.BusinessRule;
                        break;
                    case "userTask":
                        node.Type = ActivitySimNodeType.User;
                        break;
                }

                LinkSequenceFlows(node, activity, flows);
                node.EnableStats();
                // TEMPORARY FIX: This should come from the simulation parameters or something
                node.ExecutionTime = TimeSpan.FromSeconds(new Random().NextInt64(1800));
                // TEMPORARY FIX: Introduce a hard-coded 1 resource limit per activity node
                node.SetQueueMaximums(int.MaxValue, 1);

                nodes.Add(node);
            }

            var gatewayNodes = source.Elements().Where(x => x.Name.LocalName.EndsWith("Gateway", StringComparison.OrdinalIgnoreCase));
            foreach (var gatewayNode in gatewayNodes)
            {
                GatewaySimNode node = new(sim, gatewayNode.Attribute("id").Value,
                   gatewayNode.Name.LocalName);

                switch (gatewayNode.Name.LocalName)
                {
                    case "exclusiveGateway":
                        node.Type = GatewaySimNodeType.Exclusive;
                        break;
                    case "inclusiveGateway":
                        node.Type = GatewaySimNodeType.Inclusive;
                        break;
                    case "parallelGateway":
                        node.Type = GatewaySimNodeType.Parallel;
                        break;
                }

                // TEMPORARY FIX: This should come from the simulation parameters or something
                node.SetGatewayProbabilityDistribution(new FaaSDES.Sim.Nodes.GatewayProbabilityDistributions.Random());
                LinkSequenceFlows(node, gatewayNode, flows);
                node.EnableStats();
                nodes.Add(node);
            }

            // find all Events
            var eventNodes = source.Elements().Where(x => x.Name.LocalName.EndsWith("Event", StringComparison.OrdinalIgnoreCase));
            foreach (var eventNode in eventNodes)
            {
                string nodeName = string.Empty;

                if (eventNode.Attribute("name") != null)
                    nodeName = eventNode.Attribute("name").Value;

                EventSimNode node = new(sim, eventNode.Attribute("id").Value,
                   nodeName);

                switch (eventNode.Name.LocalName)
                {
                    case "intermediateThrowEvent":
                        node.Type = EventSimNodeType.IntermediateThrow;
                        break;
                    case "intermediateCatchEvent":
                        node.Type = EventSimNodeType.IntermediateCatch;
                        break;
                    case "endEvent":
                        node.Type = EventSimNodeType.End;
                        break;
                    case "startEvent":
                        node.Type = EventSimNodeType.Start;
                        break;
                }

                LinkSequenceFlows(node, eventNode, flows);

                node.EnableStats();

                if (node.Type != EventSimNodeType.Start)
                    nodes.Add(node);
            }

            // not supported at the moment: textAnnotation, association

            // Get all the sequence flow information first. When building nodes, we can refer back to this list to get
            // the information of the flow.

            foreach (var flow in flows)
            {

                var sourceNodes = nodes.Where(x => x.OutboundFlows.Any(y => y.Id == flow.Attribute("id").Value));

                if (sourceNodes.Count() != 1)
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

        /// <summary>
        /// Link the Target and Source nodes based on the SequenceFlows present in the BPMN XML file.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeSource"></param>
        /// <param name="flows"></param>
        private void LinkSequenceFlows(SimNodeBase node, XElement nodeSource, IEnumerable<XElement> flows)
        {
            foreach (var inboundFlowId in nodeSource.Elements().Where(x => x.Name.LocalName == "incoming").Select(x => x.Value))
            {
                var singleFlow = flows.Single(x => x.Attribute("id").Value == inboundFlowId);
                SequenceFlow newFlow = new(singleFlow.Attribute("sourceRef").Value, node.Id, inboundFlowId);
                newFlow.TargetNode = node;
                ((List<SequenceFlow>)node.InboundFlows).Add(newFlow);
                // how to set sourceNode here?
            }

            foreach (var outboundFlowId in nodeSource.Elements().Where(x => x.Name.LocalName == "outgoing").Select(x => x.Value))
            {
                var singleFlow = flows.Single(x => x.Attribute("id").Value == outboundFlowId);
                SequenceFlow newFlow = new(node.Id, singleFlow.Attribute("targetRef").Value, outboundFlowId);
                newFlow.SourceNode = node;
                ((List<SequenceFlow>)node.OutboundFlows).Add(newFlow);
                // how to set targetNode here?
            }
        }

        /// <summary>
        /// Initializes the provided <see cref="XElement"/>'s properties.
        /// </summary>
        /// <param name="source">The <see cref="XElement"/> whose properties should be returned.</param>
        /// <returns>A list of <see cref="BpmnProperty"/>.</returns>
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

        #endregion

        #region Fields

        private XElement SourceBpmn;
        private XNamespace BpmnNamespace;
        internal IEnumerable<BpmnProperty> Properties;

        #endregion
    }

    internal class BpmnProperty
    {
        /// <summary>
        /// An identified for this property.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// This property's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The structural reference number for this property.
        /// </summary>
        public string StructureRef { get; set; }

        /// <summary>
        /// Signifies whether this property is a collection.
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="structureRef"></param>
        /// <param name="isCollection"></param>
        public BpmnProperty(string id, string name, string structureRef, bool isCollection)
        {
            Id = id;
            Name = name;
            StructureRef = structureRef;
            IsCollection = isCollection;
        }
    }
}
