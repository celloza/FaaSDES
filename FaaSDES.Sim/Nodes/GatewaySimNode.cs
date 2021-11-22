using FaaSDES.Sim.NodeStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class GatewaySimNode : SimNodeBase
    {
        public GatewaySimNodeType Type { get; set; }

        public new GatewaySimNodeStats Stats { get; set; }

        public override void EnableStats()
        {
            if (!_isStatsEnabled)
            {
                _isStatsEnabled = true;
                Stats = new();
            }
        }

        #region Constructors

        public GatewaySimNode(string id, string name,
                IEnumerable<SequenceFlow> inboundFlows, IEnumerable<SequenceFlow> outboundFlows)
            : base(id, name, inboundFlows, outboundFlows)
        { }

        public GatewaySimNode(string id, string name)
           : base(id, name, null, null)
        {
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
        }

        public GatewaySimNode(Simulation simulation, string id, string name, GatewaySimNodeType type)
            : base(simulation, id, name)
        {
            Type = type;
        }

        #endregion

        
    }
}
