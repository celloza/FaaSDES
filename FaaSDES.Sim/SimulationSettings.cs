namespace FaaSDES.Sim
{
    /// <summary>
    /// A collection of settings for a specific <see cref="Simulation"/>.
    /// </summary>
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

        /// <summary>
        /// Provides a textual representation of this <see cref="SimulationSettings"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Simulation settings: \n\r" +
                $"Maximum iterations: {MaximumIterations} \n\r" +
                $"Start time: {StartDateTime.ToShortDateString()} \n\r" +
                $"End time: {EndDateTime.ToShortDateString()} \n\r" +
                $"Time factor: {TimeFactor} \n\r" +
                $"Maximum time tokens will wait for: {TokenMaxQueueTime}";
        }
    }
}
