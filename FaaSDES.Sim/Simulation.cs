using FaaSDES.Sim.Nodes;
using FaaSDES.Sim.Tokens;
using FaaSDES.Sim.Tokens.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim
{
    /// <summary>
    /// A single experiment for the provided <see cref="Simulator"/>.
    /// </summary>
    public class Simulation
    {
        /// <summary>
        /// A list of <see cref="ISimNode"/>s through which each generated
        /// <see cref="ISimToken"/> will progress for this <see cref="Simulation"/>.
        /// </summary>
        public IEnumerable<ISimNode> Nodes { get; set; }

        /// <summary>
        /// The first node in the process. Used as the starting place for new 
        /// <see cref="ISimToken"/>s.
        /// </summary>
        public StartSimNode StartNode { get; internal set; }

        /// <summary>
        /// The <see cref="Simulation"/>'s current state.
        /// </summary>
        public SimulationState State { get; set; }        

        /// <summary>
        /// Executes this simulation.
        /// </summary>
        public void Execute()
        {
            // On each simulation cycle:
            //
            // 0. Update the SimulationState
            // 1. Generate the requisite tokens using the token generator
            // 2. Iterate through all tokens, and either:
            // 2.1 Abandon
            // 2.2 Stay where it is
            // 2.3 Move to the next node
            // 3. Determine if simulation should continue

            // 0. Update the SimulationState
            State.CurrentIteration++;
            State.CurrentDateTime += _settings.TimeFactor;

            // Update all tokens' age and queue parameters
            foreach (SimNodeBase node in Nodes)
            {
                foreach (NodeQueueItem queueItem in node.WaitingQueue)
                {
                    queueItem.CyclesInQueue++;
                    (queueItem.TokenInQueue as SimToken).Age++;
                }
                foreach (NodeQueueItem queueItem in node.ExecutionQueue)
                {
                    queueItem.CyclesInQueue++;
                    (queueItem.TokenInQueue as SimToken).Age++;
                }
            }

            // 1. Generate the requisite tokens using the token generator
            if (StartNode.WaitingQueue.SpaceInQueue > 0)
            {
                var tokensToQueue = _tokenGenerator.GetNextTokens(State);
                var placesInQueue = StartNode.WaitingQueue.SpaceInQueue;

                if (tokensToQueue.Count() > placesInQueue)
                {
                    var tokensToAdd = tokensToQueue.Take(placesInQueue);
                    foreach (var token in tokensToAdd)
                    {
                        StartNode.WaitingQueue.AddTokenToQueue(token);
                    }

                    if (StartNode.IsStatsEnabled)
                    {
                        foreach (var token in tokensToQueue.Except(tokensToAdd))
                        {
                            //TODO: Possibly log detailed information about the lost
                            // queue events to an Azure Table.

                            StartNode.Stats.NumberOfQueueOverflows += 1;
                        }
                    }
                }
                else
                {
                    foreach (var token in tokensToQueue)
                        StartNode.WaitingQueue.AddTokenToQueue(token);
                }

                if (StartNode.IsStatsEnabled && StartNode.WaitingQueue.QueueLength > StartNode.Stats.MaximumTokensInQueue)
                    StartNode.Stats.MaximumTokensInQueue = StartNode.WaitingQueue.QueueLength;
            }

            // 2. Iterate through all tokens and take the relevant actions            
            foreach (SimNodeBase node in Nodes)
            {
                // 2.1 Abandon tokens
                var abandoningTokens = node.WaitingQueue.DequeueAbandoningTokens();

                if (node.IsStatsEnabled)
                    node.Stats.NumberOfQueueAbandons += abandoningTokens.Count();

                // Iterate through the tokens being executed, and determine if they are done
                foreach (NodeQueueItem queueItem in node.ExecutionQueue)
                {
                    if ((queueItem.CyclesInQueue * _settings.TimeFactor) >= node.ExecutionTime)
                    {
                        // Token has been serviced long enough, move it to the next step
                        SimNodeBase targetNode = null;
                        
                        switch (node)
                        {
                            case ActivitySimNode:
                            case EventSimNode:
                                targetNode = node.OutboundFlows.First().TargetNode as SimNodeBase;                                
                                break;
                            case GatewaySimNode g:
                                targetNode = g.SelectOutboundNode().TargetNode as SimNodeBase;
                                break;                            
                        }

                        ArgumentNullException.ThrowIfNull(targetNode);

                        if (targetNode.WaitingQueue.SpaceInQueue > 0)
                        {
                            //There is space in the next node's waiting queue, so move this token there
                            // First, update the statistics for this node
                            if (node.IsStatsEnabled)
                                node.Stats.NumberOfExecutions++;

                            var completedToken = node.ExecutionQueue.DequeueToken(queueItem);
                            targetNode.WaitingQueue.AddTokenToQueue(completedToken);
                        }
                        else
                        {
                            // No space in the next node's queue, so do nothing.
                            // What's expected to happen is that the token stays in the ExecutionQueue of the
                            // current node, and keeps incrementing its execution time
                        }
                    }
                }

                // Check if the node has enough resources to service another token
                if (node.ExecutionQueue.SpaceInQueue > 0)
                {
                    // Move items from the WaitingQueue into the ServicingQueue
                    for (int i = 0; i <= node.ExecutionQueue.SpaceInQueue; i++)
                    {
                        node.ExecutionQueue.AddTokenToQueue(node.WaitingQueue.DequeueTokenNextInLine());
                    }
                }
            }

            // 3. Determine if simulation should continue
            if (State.CurrentDateTime >= _settings.EndDateTime)
            {
                // end simulation based on end date
            }

            if (State.CurrentIteration >= _settings.MaximumIterations)
            {
                // end simulation based on max iterations
            }
        }

        /// <summary>
        /// Creates an instance of a simulation, based on the provided <see cref="Simulator"/>.
        /// </summary>
        /// <param name="simulator"></param>
        public Simulation(Simulator simulator, ISimTokenGenerator tokenGenerator,
            SimulationSettings settings)
        {
            _tokenGenerator = tokenGenerator;
            _settings = settings;
            _simulator = simulator;

            State = new SimulationState()
            {
                CurrentDateTime = settings.StartDateTime,
                CurrentIteration = 0
            };
        }

        private readonly ISimTokenGenerator _tokenGenerator;
        private readonly SimulationSettings _settings;
        private readonly Simulator _simulator;
    }

}
