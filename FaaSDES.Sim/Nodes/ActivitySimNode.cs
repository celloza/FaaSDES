using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class ActivitySimNode : SimNodeBase
    {
        public int ExecutionTime { get; set; }

        public ActivitySimNode(string id, string name,
            IEnumerable<SequenceFlow> inboundFlows, IEnumerable<SequenceFlow> outboundFlows)
            : base(id, name, inboundFlows, outboundFlows)
        {


        }
        public ActivitySimNode(string id, string name)
            : base(id, name, null, null)
        {
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
        }

        /// <summary>
        /// Creates an instance of an <see cref="ActivitySimNode"/> associated to the
        /// provided <see cref="Simulation"/>.
        /// </summary>
        /// <param name="simulation">The <see cref="Simulation"/> this <see cref="ActivitySimNode"/>
        /// is associated to.</param>
        /// <param name="executionTime">The time (in seconds) that this activity takes to
        /// complete.</param>
        public ActivitySimNode(Simulation simulation, string id, string name, int executionTime)
           : base(simulation, id, name)
        {
            ExecutionTime = executionTime;
        }
    }
}
