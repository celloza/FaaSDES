using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.NodeStatistics
{
    public class EventStatistic
    {
        public DateTime DateOfOccurence { get; set; }

        public EventStatisticType Type { get; set; }

        public string NodeName { get; set; }

        public EventStatistic(DateTime dateOfOccurence, EventStatisticType type, string nodeName)
        {
            DateOfOccurence = dateOfOccurence;
            Type = type;
            NodeName = nodeName;
        }
    }
}
