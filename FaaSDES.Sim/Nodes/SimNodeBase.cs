using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public abstract class SimNodeBase : ISimNode
    {
        /// <summary>
        /// Signifies the maximum number of tokens that can queue at this 
        /// specific SimNode.
        /// </summary>
        public int MaxQueueLength { get; set; }

        /// <summary>
        /// The next <see cref="ISimNode"/> in the process.
        /// </summary>
        public ISimNode? NextNode { get; set; }

        /// <summary>
        /// The previous <see cref="ISimNode"/> in the process.
        /// </summary>
        public ISimNode? PreviousNode { get; set; }

        /// <summary>
        /// Contains a list of all the inbound flows targeting this <see cref="SimNodeBase"/>.
        /// </summary>
        public IEnumerable<SequenceFlow> InboundFlows { get; set; }

        /// <summary>
        /// Contains a list of all the outbound flows eminating from this
        /// <see cref="SimNodeBase"/>.
        /// </summary>
        public IEnumerable<SequenceFlow> OutboundFlows { get; set; }
        
        public string Id { get; set; }

        public string Name { get; set; }

        public SimNodeBase(string id, string name)
        {
            Id = id;
            Name = name;
            InboundFlows = new List<SequenceFlow>();
            OutboundFlows = new List<SequenceFlow>();
        }

        /// <summary>
        /// Creates an instance of <see cref="SimNodeBase"/> with the associated
        /// <see cref="Simulation"/>.
        /// </summary>
        /// <param name="simulation"></param>
        public SimNodeBase(Simulation simulation, string id, string name)
        {
            Simulation = simulation;
            Id = id;
            Name = name;
        }

        public SimNodeBase(string id, string name, IEnumerable<SequenceFlow> 
            inboundFlows, IEnumerable<SequenceFlow> outboundFlows)
        {
            Id = id;
            Name = name;
            InboundFlows = inboundFlows;
            OutboundFlows = outboundFlows;
        }

        /// <summary>
        /// The <see cref="Simulation"/> that this <see cref="ISimNode"/> belongs to.
        /// </summary>
        public Simulation Simulation { get; set; }

        /// <summary>
        /// Clears out all tokens in the queue.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void PurgeQueue()
        {
            throw new NotImplementedException();
        }
    }
}
