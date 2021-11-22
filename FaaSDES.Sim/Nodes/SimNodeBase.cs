using FaaSDES.Sim.NodeStatistics;
using FaaSDES.Sim.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public abstract class SimNodeBase : ISimNode, ISimNodeExecutionHandler
    {

        /// <summary>
        /// Contains a list of all the inbound flows targeting this <see cref="SimNodeBase"/>.
        /// </summary>
        public IEnumerable<SequenceFlow> InboundFlows { get; set; }

        /// <summary>
        /// Contains a list of all the outbound flows eminating from this
        /// <see cref="SimNodeBase"/>.
        /// </summary>
        public IEnumerable<SequenceFlow> OutboundFlows { get; set; }
        
        /// <summary>
        /// The identifier for this Node from the BPMN XML's Id attribute.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The identifier for this Node from the BPMN XML's Name attribute.
        /// </summary>
        public string Name { get; set; }       

        /// <summary>
        /// The <see cref="Simulation"/> that this <see cref="ISimNode"/> belongs to.
        /// </summary>
        public Simulation Simulation { get; set; }

        ///// <summary>
        ///// Checks whether there is space in the queue to enqueue another <see cref="Token"/>.
        ///// </summary>
        //public bool CanEnqueueToken
        //{
        //    get
        //    {
        //        return Tokens != null && Tokens.Count < MaxQueueLength;
        //    }
        //}

        ///// <summary>
        ///// Returns the number of <see cref="Tokens"/> that can be queued.
        ///// </summary>
        //public int PlacesInQueue
        //{
        //    get
        //    {
        //        return MaxQueueLength - Tokens.Count;
        //    }
        //}

        /// <summary>
        /// The <see cref="Task"/> to be executed when this <see cref="SimNodeBase"/> is
        /// executed.
        /// </summary>
        public Task ExecutionTask { get; set; }

        /// <summary>
        /// Signifies whether statistics should be gathered for this node.
        /// </summary>
        public bool IsStatsEnabled
        {
            get { return _isStatsEnabled; }
        }

        /// <summary>
        /// Contains statistics pertaining to this <see cref="ISimNode"/>. Should be 
        /// overriden in classes inheriting from this class to provide more topical
        /// statistics.
        /// </summary>
        public SimNodeStatsBase Stats { get; set; }

        ///// <summary>
        ///// Clears out all tokens in the queue.
        ///// </summary>
        ///// <exception cref="NotImplementedException"></exception>
        //public void PurgeQueue()
        //{
        //    throw new NotImplementedException();
        //}
        public NodeQueue TokenQueue { get; set; }

        /// <summary>
        /// Executes the pre-set execution task.
        /// </summary>
        /// <param name="node"></param>
        public virtual void Execute(ISimNode node)
        {
            ExecutionTask = new Task(() => Execute(this));
            ExecutionTask.Start();
        }

        public abstract void EnableStats();

        #region Constructors

        public SimNodeBase(string id, string name)
        {
            Id = id;
            Name = name;
            InboundFlows = new List<SequenceFlow>();
            OutboundFlows = new List<SequenceFlow>();            
        }

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

        #endregion

        internal bool _isStatsEnabled;

    }
}
