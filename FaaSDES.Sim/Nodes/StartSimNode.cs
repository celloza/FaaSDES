using FaaSDES.Sim.NodeStatistics;
using FaaSDES.Sim.Tokens.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class StartSimNode : SimNodeBase
    {
        public new StartSimNodeStats Stats { get; set; }

        public override void EnableStats()
        {
            if (!_isStatsEnabled)
            {
                _isStatsEnabled = true;
                Stats = new();
            }
        }

        #region Constructors

        public StartSimNode(Simulation simulation, string id, string name)
            : base(simulation, id, name)
        {
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
        }        

        #endregion
    }
}
