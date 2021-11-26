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

        /// <summary>
        /// Creates a new <see cref="Simulator"/> instance with the provided 
        /// <see cref="ISimTokenGenerator"/> and <see cref="SimulationSettings"/>.
        /// </summary>
        /// <param name="tokenGenerator">An instance of <see cref="ISimTokenGenerator"/>, 
        /// responsible for the generation of tokens for the simulation.</param>
        /// <param name="settings">An instance of <see cref="SimulationSettings"/> that
        /// contains settings controlling the <see cref="Simulator"/>.</param>
        public static Simulator FromBpmnXML(FileStream streamToFile)
        {
            if (streamToFile == null)
                throw new ArgumentNullException(nameof(streamToFile));

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
        /// Initializes the class with the provided BPMN XML FileStream object.
        /// </summary>
        /// <param name="streamToFile"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void BuildSimulatorFromBpmnXml(FileStream streamToFile)
        {
            if (streamToFile == null)
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

        /// <summary>
        /// Creates a new <see cref="Simulation"/> instance based on this <see cref="Simulator"/>.
        /// </summary>
        /// <returns></returns>
        public Simulation NewSimulationInstance(SimulationSettings settings, ISimTokenGenerator tokenGenerator)
        {
            Simulation sim = new(this, tokenGenerator, settings);

            sim.Nodes = BuildAndLinkNodes(SourceBpmn, sim, out StartSimNode startNode);

            sim.StartNode = startNode;
            return sim;
        }

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
                ActivitySimNode node = new(sim, activity.Attribute("id").Value,
                   activity.Name.LocalName);

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
                nodes.Add(node);
            }

            // find all exclusiveGateways
            var exclusiveGateways = source.Elements().Where(x => x.Name == BpmnNamespace + "exclusiveGateway");
            foreach (var exclusiveGateway in exclusiveGateways)
            {
                GatewaySimNode node = new(sim, exclusiveGateway.Attribute("id").Value,
                   exclusiveGateway.Name.LocalName, GatewaySimNodeType.Exclusive);
                LinkSequenceFlows(node, exclusiveGateway, flows);
                // TEMPORARY FIX: This should come from the simulation parameters or something
                node.SetGatewayProbabilityDistribution(new FaaSDES.Sim.Nodes.GatewayProbabilityDistributions.Random());
                nodes.Add(node);
            }

            // find all inclusiveGateways
            var inclusiveGateways = source.Elements().Where(x => x.Name == BpmnNamespace + "inclusiveGateway");
            foreach (var inclusiveGateway in inclusiveGateways)
            {
                GatewaySimNode node = new(sim, inclusiveGateway.Attribute("id").Value,
                   inclusiveGateway.Name.LocalName, GatewaySimNodeType.Inclusive);
                LinkSequenceFlows(node, inclusiveGateway, flows);
                // TEMPORARY FIX: This should come from the simulation parameters or something
                node.SetGatewayProbabilityDistribution(new FaaSDES.Sim.Nodes.GatewayProbabilityDistributions.Random());
                nodes.Add(node);
            }

            // find all parallelGateways
            var parallelGateways = source.Elements().Where(x => x.Name == BpmnNamespace + "parallelGateway");
            foreach (var parallelGateway in parallelGateways)
            {
                GatewaySimNode node = new(sim, parallelGateway.Attribute("id").Value,
                   parallelGateway.Name.LocalName, GatewaySimNodeType.Parallel);
                LinkSequenceFlows(node, parallelGateway, flows);
                // TEMPORARY FIX: This should come from the simulation parameters or something
                node.SetGatewayProbabilityDistribution(new FaaSDES.Sim.Nodes.GatewayProbabilityDistributions.Random());
                nodes.Add(node);
            }

            // find all Events
            var eventNodes = source.Elements().Where(x => x.Name.LocalName.EndsWith("Event", StringComparison.OrdinalIgnoreCase));
            foreach (var eventNode in eventNodes)
            {
                EventSimNode node = new(sim, eventNode.Attribute("id").Value,
                   eventNode.Name.LocalName);

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

        private Simulator() { }

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
