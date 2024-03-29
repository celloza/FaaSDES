﻿namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Class representing the Activity/Task BPMN symbol.
    /// </summary>
    public class ActivitySimNode : SimNodeBase
    {
        public ActivitySimNodeType Type { get; set; } = ActivitySimNodeType.Undefined;

        public override void EnableStats()
        {
            if (!_isStatsEnabled)
            {
                _isStatsEnabled = true;
                Stats = new();
            }
        }

        #region Constructors

        public ActivitySimNode(Simulation sim, string id, string name)
            : base(id, name, null, null)
        {
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
            Simulation = sim;
        }

        #endregion
    }
}
