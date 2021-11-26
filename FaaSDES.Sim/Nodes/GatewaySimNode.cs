using FaaSDES.Sim.Nodes.GatewayProbabilityDistributions;
using FaaSDES.Sim.NodeStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Class representing the Gateway BPMN symbol.
    /// </summary>
    public class GatewaySimNode : SimNodeBase
    {

        public GatewaySimNodeType Type { get; set; }

        /// <summary>
        /// The <see cref="GatewaySimNodeStats"/> for this node.
        /// </summary>
        public new GatewaySimNodeStats Stats { get; set; }

        public override void EnableStats()
        {
            if (!_isStatsEnabled)
            {
                _isStatsEnabled = true;
                Stats = new();
            }
        }

        /// <summary>
        /// Selects the outbound SequenceFlow based on a the probabilistic distribution set for 
        /// this <see cref="GatewaySimNode"/>.
        /// </summary>
        /// <returns></returns>
        public SequenceFlow SelectOutboundNode()
        {
            ArgumentNullException.ThrowIfNull(_gatewayProbabilityDistribution);
            return _gatewayProbabilityDistribution.ChooseSequenceFlow(OutboundFlows);
        }

        public void SetGatewayProbabilityDistribution(IGatewayProbabilityDistribution distribution)
        {
            ArgumentNullException.ThrowIfNull(distribution);
            _gatewayProbabilityDistribution = distribution;
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

        private IGatewayProbabilityDistribution _gatewayProbabilityDistribution;
    }
}
