using FaaSDES.Sim.NodeStatistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class EventSimNode : SimNodeBase
    {
        /// <summary>
        /// The <see cref="EventSimNodeStats"/> for this node.
        /// </summary>
        public new EventSimNodeStats Stats { get; set; }

        public EventSimNodeType Type { get; set; }

        public EventSimNodeTrigger Trigger { get; set; } = EventSimNodeTrigger.None;

        public override void EnableStats()
        {
            if (!_isStatsEnabled)
            {
                _isStatsEnabled = true;
                Stats = new();
            }
        }

        #region Constructors

        public EventSimNode(string id, string name,
            IEnumerable<SequenceFlow> inboundFlows, IEnumerable<SequenceFlow> outboundFlows)
            : base(id, name, inboundFlows, outboundFlows)
        { }

        public EventSimNode(string id, string name)
           : base(id, name, null, null)
        {
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
        }

        public EventSimNode(Simulation simulation, string id, string name)
           : base(simulation, id, name)
        { }

        

        #endregion
    }
}
