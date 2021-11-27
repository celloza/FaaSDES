namespace FaaSDES.Sim
{
    public class SimulationSettings
    {
        /// <summary>
        /// The maximum number of iterations that this simulator can run for.
        /// </summary>
        public int MaximumIterations { get; set; } = int.MaxValue;

        /// <summary>
        /// The start date of the simulation. Especially important if the 
        /// <see cref="TimerSimTokenGenerator"/> is used.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The end date of the simulation. Especially important if the 
        /// <see cref="TimerSimTokenGenerator"/> is used.
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Contains the simulation increment. The simulation clock will be
        /// increment by this value on each cycle.
        /// </summary>
        public TimeSpan TimeFactor { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Denotes the maximum number of time that a token will remain in
        /// a queue before it needs to be abandoned.
        /// </summary>
        public TimeSpan TokenMaxQueueTime { get; set; } = TimeSpan.MaxValue;
    }
}
