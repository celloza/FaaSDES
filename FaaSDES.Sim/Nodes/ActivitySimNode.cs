using FaaSDES.Sim.NodeStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class ActivitySimNode : SimNodeBase
    {
        /// <summary>
        /// The time it takes to execute this activity for a single token.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Signifies the number of resources available to this Activity. If more than one 
        /// resource is available, tokens can be executed in parallel (i.e. 3 resources, 3 
        /// parallel executions).
        /// </summary>
        public int ResourcesAvailable { get; set; }

        public new EventSimNodeStats Stats { get; set; }

        public override void EnableStats()
        {
            if (!_isStatsEnabled)
            {
                _isStatsEnabled = true;
                Stats = new();
            }
        }

        #region Constructors

        public ActivitySimNode(string id, string name,
            IEnumerable<SequenceFlow> inboundFlows, IEnumerable<SequenceFlow> outboundFlows)
            : base(id, name, inboundFlows, outboundFlows)
        { }

        public ActivitySimNode(Simulation sim, string id, string name)
            : base(id, name, null, null)
        {
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
            Simulation = sim;
        }

        /// <summary>
        /// Creates an instance of an <see cref="ActivitySimNode"/> associated to the
        /// provided <see cref="Simulation"/>.
        /// </summary>
        /// <param name="simulation">The <see cref="Simulation"/> this <see cref="ActivitySimNode"/>
        /// is associated to.</param>
        /// <param name="executionTime">The time (in seconds) that this activity takes to
        /// complete.</param>
        public ActivitySimNode(Simulation simulation, string id, string name, TimeSpan executionTime)
           : base(simulation, id, name)
        {
            ExecutionTime = executionTime;
        }       

        #endregion
    }
}
