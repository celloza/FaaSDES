using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.NodeStatistics
{
    public class SimNodeStats
    {
        ///// <summary>
        ///// Keeps a record of the maximum number of items that were in the queue
        ///// at any one time.
        ///// </summary>
        //public int MaximumTokensInQueue
        //{
        //    get { return _maximumTokensInQueue; }
        //    set { _maximumTokensInQueue = value; }
        //}

        ///// <summary>
        ///// Keeps a record of how many times this <see cref="ISimNode"/> has
        ///// been executed.
        ///// </summary>
        //public int NumberOfExecutions
        //{
        //    get { return _numberOfExecutions; }
        //    set { _numberOfExecutions = value; }

        //}

        ///// <summary>
        ///// Keeps a record of how many <see cref="ISimToken"/> could not be serviced by this
        ///// node because the queue was full.
        ///// </summary>
        //public int NumberOfQueueOverflows
        //{
        //    get { return _numberOfQueueOverflows; }
        //    set { _numberOfQueueOverflows = value;}
        //}

        ///// <summary>
        ///// Keeps a record of how many <see cref="ISimToken"/>s where abandoned while waiting
        ///// in this queue.
        ///// </summary>
        //public int NumberOfQueueAbandons
        //{
        //    get { return _numberOfQueueAbandons; }
        //    set { _numberOfQueueAbandons = value; }
        //}

        //private int _numberOfExecutions = 0;
        //private int _maximumTokensInQueue = 0;
        //private int _numberOfQueueOverflows = 0;
        //private int _numberOfQueueAbandons = 0;

        public IEnumerable<EventStatistic> EventStatistics { get; set; }


        public SimNodeStats()
        {
            EventStatistics = new List<EventStatistic>();
        }

        public EventStatistic AddEventStat(DateTime dateOfOccurence, EventStatisticType type, string nodeName)
        {
            var eventStat = new EventStatistic(dateOfOccurence, type, nodeName);
            (EventStatistics as List<EventStatistic>).Add(eventStat);
            return eventStat;
        }        
    }
}
