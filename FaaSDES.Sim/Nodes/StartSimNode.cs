namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Class representing the Start BPMN symbol.
    /// </summary>
    public class StartSimNode : SimNodeBase
    {
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
            ExecutionTime = TimeSpan.Zero;
        }        

        #endregion
    }
}
