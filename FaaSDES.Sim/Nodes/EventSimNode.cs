namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Class representing the Event BPMN symbol.
    /// </summary>
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
