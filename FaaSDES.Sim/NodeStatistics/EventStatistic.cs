namespace FaaSDES.Sim.NodeStatistics
{
    /// <summary>
    /// Class encapsulating a single statistical event captured during the
    /// execution of a <see cref="Simulation"/>.
    /// </summary>
    public class EventStatistic
    {
        /// <summary>
        /// When the event occurred.
        /// </summary>
        public DateTime DateOfOccurence { get; set; }

        /// <summary>
        /// The name of the node on which this event occurred.
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// The <see cref="EventStatisticType"/> of this event.
        /// </summary>
        public EventStatisticType Type
        {
            get;
            set;
        }

        /// <summary>
        /// The identifier for the owning <see cref="Simulation"/>.
        /// </summary>
        public Guid SimulationId { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="dateOfOccurence"></param>
        /// <param name="type"></param>
        /// <param name="nodeName"></param>
        /// <param name="simulationId"></param>
        public EventStatistic(DateTime dateOfOccurence, EventStatisticType type, 
            string nodeName, Guid simulationId)
        {
            DateOfOccurence = dateOfOccurence;
            Type = type;
            NodeName = nodeName;
            SimulationId = simulationId;
        }
    }
}
