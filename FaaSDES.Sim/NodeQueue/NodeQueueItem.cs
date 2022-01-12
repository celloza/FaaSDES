using FaaSDES.Sim.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// A single instance of an item in a <see cref="NodeQueue"/>.
    /// </summary>
    public class NodeQueueItem
    {
        /// <summary>
        /// The <see cref="ISimToken"/> waiting in queue.
        /// </summary>
        public ISimToken TokenInQueue { get; set; }

        /// <summary>
        /// The number of simulation cycles that this item has been in the 
        /// queue.
        /// </summary>
        public int CyclesInQueue { get; set; } = 0;
    }
}
