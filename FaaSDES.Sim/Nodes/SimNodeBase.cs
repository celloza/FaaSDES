using FaaSDES.Sim.NodeStatistics;
using FaaSDES.Sim.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Base class for a simulation node.
    /// </summary>
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

        /// <summary>
        /// The <see cref="NodeQueue"/> containing all the tokens in the queue to be 
        /// serviced by this node.
        /// </summary>
        public NodeQueue WaitingQueue { get; set; }

        /// <summary>
        /// The <see cref="NodeQueue"/> containing all the tokens that are currently being
        /// serviced by the <see cref="SimNodeBase"/>. The maximum queue length for this 
        /// list denotes the number of available resources to service the tokens in 
        /// parallel.
        /// </summary>
        public NodeQueue ExecutionQueue { get; set; }

        /// <summary>
        /// Denotes the amount of time it takes to service a token in this node. This 
        /// <see cref="TimeSpan"/> should be converted to number of simulation cycles, taking
        /// the time factor into account. By default, this is set to 0.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; } = new TimeSpan(0, 0, 0);

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

        /// <summary>
        /// Sets the limits for the two <see cref="NodeQueue"/> queues.
        /// </summary>
        /// <param name="maxTokensWaiting">Sets the maximum number of tokens that could
        /// wait in the queue to be serviced by this <see cref="SimNodeBase"/>.</param>
        /// <param name="resourcesAvailable">Sets the maximum number of tokens that this
        /// <see cref="SimNodeBase"/> can handle in parallel, referred to as the
        /// available resources in BPMN.</param>
        public void SetQueueMaximums(int maxTokensWaiting, int resourcesAvailable)
        {
            if (WaitingQueue.HasItemsInQueue || ExecutionQueue.HasItemsInQueue)
                throw new InvalidOperationException("You cannot change the max queue lengths " +
                    "when there are items in the queues.");

            WaitingQueue = new NodeQueue(maxTokensWaiting);
            ExecutionQueue = new NodeQueue(resourcesAvailable);
        }

        #region Constructors

        /// <summary>
        /// Creates an instance of a <see cref="SimNodeBase"/>.
        /// 
        /// Defaults are set for some parameters:
        /// Maximum number of tokens allowed to wait: <see cref="int.MaxValue"/>.
        /// Number of parallel workers (resources available): 1
        /// TIme to service a token: 5 minutes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public SimNodeBase(string id, string name)
        {
            Id = id;
            Name = name;
            InboundFlows = new List<SequenceFlow>();
            OutboundFlows = new List<SequenceFlow>();
            
            WaitingQueue = new NodeQueue();
            ExecutionQueue = new NodeQueue();
            ExecutionTime = new TimeSpan(0, 5, 0);
        }

        public SimNodeBase(Simulation simulation, string id, string name)
            : this(id, name)
        { 
            Simulation = simulation;
        }

        public SimNodeBase(string id, string name, IEnumerable<SequenceFlow>
            inboundFlows, IEnumerable<SequenceFlow> outboundFlows)
            : this(id, name)
        {           
            InboundFlows = inboundFlows;
            OutboundFlows = outboundFlows;
        }

        #endregion

        internal bool _isStatsEnabled;

    }
}
