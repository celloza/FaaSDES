using FaaSDES.Sim.NodeStatistics;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Functions.Statistics
{
    public class EventStatisticTableEntity : TableEntity
    {
        private EventStatistic statistic;

        public DateTime DateOfOccurence 
        { 
            get { return statistic.DateOfOccurence; }
            set { statistic.DateOfOccurence = value; }
        }

        public string NodeName 
        { 
            get { return statistic.NodeName; }
            set { statistic.NodeName = value; }
        }

        public EventStatisticType Type
        {
            get {  return statistic.Type; }
            set { statistic.Type = value; }
        }

        public Guid SimulationId
        {
            get { return statistic.SimulationId; }
            set { statistic.SimulationId = value; }
        }

        public EventStatisticTableEntity(EventStatistic stat)
        {
            statistic = stat;
        }
    }

    public static class EventStatisticExtensions
    {
        public static EventStatisticTableEntity ToTableEntity(this EventStatistic stat)
        {
            var entity = new EventStatisticTableEntity(stat);

            entity.PartitionKey = "EventStatistic-" + stat.SimulationId;
            entity.RowKey = Guid.NewGuid().ToString();

            return entity;
        }
    }
}
