namespace FaaSDES.Sim
{
    /// <summary>
    /// Contains the current state of a single <see cref="Simulation"/>.
    /// </summary>
    public class SimulationState
    {
        /// <summary>
        /// The current hypothetical date and time of the simulation. Dates are mostly
        /// ignored, but is important to during the calculation of weekdays, which may
        /// influence token generation in token generators such as 
        /// <see cref="TimerSimTokenGenerator"/>.
        /// </summary>
        public DateTime CurrentDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// The current iteration of the Schedule Clock.
        /// </summary>
        public int CurrentIteration { get; set; } = 0;

    }
}
