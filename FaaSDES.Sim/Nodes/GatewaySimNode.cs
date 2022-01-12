using FaaSDES.Sim.Nodes.GatewayProbabilityDistributions;
using FaaSDES.Sim.NodeStatistics;

namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Class representing the Gateway BPMN symbol.
    /// </summary>
    public class GatewaySimNode : SimNodeBase
    {
        public GatewaySimNodeType Type { get; set; }

        public override void EnableStats()
        {
            if (!_isStatsEnabled)
            {
                _isStatsEnabled = true;
                Stats = new GatewaySimNodeStats();
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

        public GatewaySimNode(Simulation simulation, string id, string name)
            : base(simulation, id, name)
        {
            ExecutionTime = TimeSpan.Zero;
        }

        #endregion

        private IGatewayProbabilityDistribution _gatewayProbabilityDistribution;
    }
}
