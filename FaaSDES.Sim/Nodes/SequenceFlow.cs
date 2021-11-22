using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class SequenceFlow
    {
        public ISimNode? SourceNode { get; set; }

        public ISimNode? TargetNode { get; set; }

        public string SourceNodeName { get; set; } = string.Empty;

        public string TargetNodeName { get; set; } = string.Empty;

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
