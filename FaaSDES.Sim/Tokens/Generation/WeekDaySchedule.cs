namespace FaaSDES.Sim.Tokens.Generation
{
    /// <summary>
    /// Class to strongly-type a daily schedule.
    /// </summary>
    public class WeekDaySchedule
    {
        private readonly Dictionary<DayOfWeek, bool> activeDays;

        public bool ActiveOnMonday { get; }
        public bool ActiveOnTuesday { get; }
        public bool ActiveOnWednesday { get; }
        public bool ActiveOnThursday { get; }
        public bool ActiveOnFriday { get; }
        public bool ActiveOnSaturday { get; }
        public bool ActiveOnSunday { get; }

        /// <summary>
        /// Creates an instance of <see cref="WeekDaySchedule"/>.
        /// </summary>
        /// <param name="activeOnMonday">Set the schedule to active/inactive on Mondays.</param>
        /// <param name="activeOnTuesday">Set the schedule to active/inactive on Tuesdays.</param>
        /// <param name="activeOnWednesday">Set the schedule to active/inactive on Wednesdays.</param>
        /// <param name="activeOnThursday">Set the schedule to active/inactive on Thursdays.</param>
        /// <param name="activeOnFriday">Set the schedule to active/inactive on Fridays.</param>
        /// <param name="activeOnSaturday">Set the schedule to active/inactive on Saturdays.</param>
        /// <param name="activeOnSunday">Set the schedule to active/inactive on Sundays.</param>
        public WeekDaySchedule(bool activeOnMonday, bool activeOnTuesday, bool activeOnWednesday, 
            bool activeOnThursday, bool activeOnFriday, bool activeOnSaturday, bool activeOnSunday)
        {
            ActiveOnMonday = activeOnMonday;
            ActiveOnTuesday = activeOnTuesday;
            ActiveOnWednesday = activeOnWednesday;
            ActiveOnThursday = activeOnThursday;
            ActiveOnFriday = activeOnFriday;
            ActiveOnSaturday = activeOnSaturday;
            ActiveOnSunday = activeOnSunday;

            activeDays = new Dictionary<DayOfWeek, bool>
            {
                { DayOfWeek.Monday, activeOnMonday },
                { DayOfWeek.Tuesday, activeOnTuesday },
                { DayOfWeek.Wednesday, activeOnWednesday },
                { DayOfWeek.Thursday, activeOnThursday },
                { DayOfWeek.Friday, activeOnFriday },
                { DayOfWeek.Saturday, activeOnSaturday },
                { DayOfWeek.Sunday, activeOnSunday }
            };
        }

        /// <summary>
        /// Check whether this instance has at least one day active.
        /// </summary>
        /// <returns>True if at least one day is marked as active.</returns>
        public bool AtLeastOneDayActive()
        {
            return ActiveOnMonday | ActiveOnTuesday | ActiveOnWednesday | ActiveOnThursday 
                | ActiveOnFriday | ActiveOnSaturday | ActiveOnSunday;
        }

        /// <summary>
        /// Checks whether the schedule is active for the provided <see cref="DayOfWeek"/>.
        /// </summary>
        /// <param name="dayToCheck">The <see cref="DayOfWeek"/> to check.</param>
        /// <returns>True if the schedule is active.</returns>
        public bool IsActiveOnDayOfWeek(DayOfWeek dayToCheck)
        {
            return activeDays[dayToCheck];
        }

        public override string ToString()
        {
            return $"Daily schedule: \n\r" +
                $"Monday: {ActiveOnMonday} \n\r" +
                $"Tuesday: {ActiveOnTuesday} \n\r" +
                $"Wednesday: {ActiveOnWednesday} \n\r" +
                $"Thursday: {ActiveOnThursday} \n\r" +
                $"Friday: {ActiveOnFriday} \n\r" +
                $"Saturday: {ActiveOnSaturday} \n\r" +
                $"Sunday: {ActiveOnSunday}";
        }
    }
}
