using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class encapsulating a single statistical event captured by a 
/// <see cref="GatewaySimNode"/> during the execution of a <see cref="Simulation"/>.
/// </summary>
namespace FaaSDES.Sim.NodeStatistics
{
    public class GatewaySimNodeStats : SimNodeStats
    {
        /// <summary>
        /// Records the frequency of each outbound destination.
        /// </summary>
        public Dictionary<string, int> DestinationDistribution { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="GatewaySimNodeStats"/>.
        /// </summary>
        public GatewaySimNodeStats()
        {
            DestinationDistribution = new();
        }

        /// <summary>
        /// Increments the specified destination by 1. If if doesn't exist
        /// in the Dictionary, its added automatically.
        /// </summary>
        /// <param name="destination">The name of the destination, i.e., the
        /// Id of the chosen next <see cref="ISimNode"/>.</param>
        public void AddDestinationDistributionStat(string destination)
        {
            if(DestinationDistribution.ContainsKey(destination))
            {
                DestinationDistribution[destination] += 1;
            }
            else
            {
                DestinationDistribution.Add(destination, 1);
            }
        }
    }
}
