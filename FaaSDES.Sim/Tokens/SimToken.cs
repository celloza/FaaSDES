using FaaSDES.Sim.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Tokens
{
    /// <summary>
    /// A single instance of a <see cref="SimToken"/>. 
    /// 
    /// In a business process context, this could represent a person interacting 
    /// with the business process modelled through BPMN.
    /// </summary>
    public class SimToken : ISimToken
    {
        /// <summary>
        /// This <see cref="SimToken"/>'s status.
        /// </summary>
        public SimTokenStatus Status { get; set; }

        /// <summary>
        /// A unique identifier for this <see cref="SimToken"/>.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Where the token currently is in the process.
        /// </summary>
        public ISimNode CurrentLocation { get; set; }

        /// <summary>
        /// The maximum number of simulation cycles that this token is willing to wait
        /// in a queue. When moving to another queue, this value should be reset to 0.
        /// </summary>
        public int MaxWaitTime { get; set; } = int.MaxValue;

        /// <summary>
        /// The number of cycles a token has progressed through.
        /// </summary>
        public int Age { get; set; } = 0;

    }
}
