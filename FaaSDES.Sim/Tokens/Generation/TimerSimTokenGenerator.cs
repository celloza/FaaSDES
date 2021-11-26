namespace FaaSDES.Sim.Tokens.Generation
{
    /// <summary>
    /// SimTokenGenerator that only generates tokens within the specified <see cref="StartTime"/>
    /// and <see cref="EndTime"/>, but only on the days specified in <see cref="WeekDaySchedule"/>.
    /// </summary>
    public class TimerSimTokenGenerator : SimTokenGeneratorBase
    {
        /// <summary>
        /// The time from which this <see cref="ISimTokenGenerator"/> will generate tokens. Requests
        /// to <see cref="GetNextTokens"/> before this time (in a day) will not produce any tokens.
        /// </summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// The time to which this <see cref="ISimTokenGenerator"/> will generate tokens. Requests
        /// to <see cref="GetNextTokens"/> after this time (in a day) will not produce any tokens.
        /// </summary>
        public TimeOnly EndTime { get; set; }

        /// <summary>
        /// The days of the week for which this generator is active.
        /// </summary>
        public WeekDaySchedule DaysOfWeek { get; set; }

        /// <summary>
        /// Creates an instance of this <see cref="TimerSimTokenGenerator"/>.
        /// </summary>
        /// <param name="startTime">Sets the generator's <see cref="StartTime"/>.</param>
        /// <param name="endTime">Sets the generator's <see cref="EndTime"/>.</param>
        /// <param name="daysOfWeek">The days of the week on which this generator will
        /// produce tokens.</param>
        /// <param name="settings">Settings for the generation of tokens.</param>
        public TimerSimTokenGenerator(GenerationSettings settings, TimeOnly startTime, 
            TimeOnly endTime, WeekDaySchedule daysOfWeek)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (startTime > endTime)
                throw new ArgumentException("EndTime cannot be before StartTime");

            if (daysOfWeek == null || !daysOfWeek.AtLeastOneDayActive())
                throw new ArgumentException("Need at least one day a week to be active.");

            StartTime = startTime;
            EndTime = endTime;
            DaysOfWeek = daysOfWeek;

        }

        /// <summary>
        /// Produce the next token(s), based on the configuration of this generator.
        /// </summary>
        /// <returns>A list of <see cref="SimToken"/>.</returns>
        public override IEnumerable<SimToken> GetNextTokens(SimulationState state)
        {
            if (ShouldGenerateToken(state.CurrentDateTime))
            {
                var newTokens = new List<SimToken>();

                //TODO: generate the tokens based on _settings
                var temp = _settings.ProbabilisticDistribution;

                return newTokens.AsEnumerable();
            }
            else
            {
                return Enumerable.Empty<SimToken>();
            }                
        }

        private bool ShouldGenerateToken(DateTime dateToCheck)
        {
            if (!this.IsGeneratingTokens)
                return false;
            else
            {
                if (DaysOfWeek.IsActiveOnDayOfWeek(dateToCheck.DayOfWeek) &&
                    TimeOnly.FromDateTime(dateToCheck) > StartTime &&
                    TimeOnly.FromDateTime(dateToCheck) < EndTime)
                    return true;
                else
                    return false;
            }
        }

        private readonly GenerationSettings _settings;
    }
}
