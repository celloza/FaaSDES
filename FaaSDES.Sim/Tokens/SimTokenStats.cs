using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Tokens
{
    /// <summary>
    /// Class encapsulating the statistics captured for a specific
    /// <see cref="SimToken"/> during the execution of a 
    /// <see cref="Simulation"/>.
    /// </summary>
    public class SimTokenStats
    {
        /// <summary>
        /// The total time that a <see cref="SimToken"/> has spent
        /// waiting in a queue.
        /// </summary>
        public TimeSpan TotalWaitTime { get; set; }

        /// <summary>
        /// The duration that a <see cref="SimToken"/> took to be 
        /// completely processed during a <see cref="Simulation"/>.
        /// </summary>
        public TimeSpan TotalExecutionTime { get; set; }

        /// <summary>
        /// The date and time that a <see cref="SimToken"/> was 
        /// generated.
        /// </summary>
        public DateTime GenerationDate { get; set; }

        /// <summary>
        /// The date and time when a <see cref="SimToken"/> was 
        /// completely processed through a <see cref="Simulation"/>.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The <see cref="TokenEndReason"/> why a <see cref="SimToken"/>
        /// has completed processing.
        /// </summary>
        public TokenEndReason EndReason { get; set; }


    }
}
