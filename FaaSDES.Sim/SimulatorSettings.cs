namespace FaaSDES.Sim
{
    public class SimulatorSettings
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
    }
}
