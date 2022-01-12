namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Class representing a sequence flow in a BPMN diagram.
    /// </summary>
    public class SequenceFlow
    {
        /// <summary>
        /// The source of this <see cref="SequenceFlow"/>.
        /// </summary>
        public ISimNode? SourceNode { get; set; }

        /// <summary>
        /// The target of this <see cref="SequenceFlow"/>.
        /// </summary>
        public ISimNode? TargetNode { get; set; }

        /// <summary>
        /// The source node's name.
        /// </summary>
        public string SourceNodeName { get; set; } = string.Empty;

        /// <summary>
        /// The target node's name
        /// </summary>
        public string TargetNodeName { get; set; } = string.Empty;

        /// <summary>
        /// An identifier for this <see cref="SequenceFlow"/>.
        /// </summary>
        public string Id { get; set; }

        public SequenceFlow(ISimNode sourceNode, ISimNode targetNode, string id)
        {
            SourceNode = sourceNode;
            TargetNode = targetNode;
            Id = id;
        }

        public SequenceFlow(string sourceNodeName, string targetNodeName, string id)
        {
            SourceNodeName = sourceNodeName;
            TargetNodeName = targetNodeName;
            Id = id;
        }
    }
}
