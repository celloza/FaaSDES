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

        public EventSimNode(Simulation simulation, string id, string name)
           : base(simulation, id, name)
        {
            ExecutionTime = TimeSpan.Zero;
        }       

        #endregion
    }
}
