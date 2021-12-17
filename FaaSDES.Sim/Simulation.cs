using FaaSDES.Sim.Nodes;
using FaaSDES.Sim.NodeStatistics;
using FaaSDES.Sim.Tokens;
using FaaSDES.Sim.Tokens.Generation;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FaaSDES.Sim
{
    /// <summary>
    /// A single experiment for the provided <see cref="Simulator"/>.
    /// </summary>
    public class Simulation
    {
        #region Public Properties

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

        public IEnumerable<ISimToken> CompletedTokens { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes this simulation.
        /// </summary>
        public void Execute()
        {
            bool continueExecution = true;
            _tokenGenerator.Start();

            while (continueExecution)
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
                        (queueItem.TokenInQueue as SimToken).Stats.TotalWaitTime += _settings.TimeFactor;


                    }
                    foreach (NodeQueueItem queueItem in node.ExecutionQueue)
                    {
                        queueItem.CyclesInQueue++;
                        (queueItem.TokenInQueue as SimToken).Age++;
                        (queueItem.TokenInQueue as SimToken).Stats.TotalWaitTime += _settings.TimeFactor;
                    }
                }

                // 1. Generate the requisite tokens using the token generator
                if (StartNode.WaitingQueue.SpaceInQueue > 0)
                {
                    var tokensToQueue = _tokenGenerator.GetNextTokens(State);

                    if (tokensToQueue.Count() > 0)
                    {
                        var placesInQueue = StartNode.WaitingQueue.SpaceInQueue;

                        if (tokensToQueue.Count() < placesInQueue)
                        {
                            var tokensToAdd = tokensToQueue.Take(placesInQueue);
                            foreach (var token in tokensToAdd)
                            {
                                token.MaxWaitTime = _settings.TokenMaxQueueTime;
                                token.CurrentLocation = StartNode;
                                token.Status = SimTokenStatus.InQueue;
                                token.Stats.GenerationDate = State.CurrentDateTime;
                                StartNode.WaitingQueue.AddTokenToQueue(token);
                                if (StartNode.IsStatsEnabled)
                                {
                                    StartNode.Stats.AddEventStat(
                                        State.CurrentDateTime,
                                        NodeStatistics.EventStatisticType.TokenJoinedWaitingQueue,
                                        StartNode.Id);
                                }
                            }

                            if (StartNode.IsStatsEnabled)
                            {
                                foreach (var token in tokensToQueue.Except(tokensToAdd))
                                {
                                    //TODO: Possibly log detailed information about the lost

                                    //StartNode.Stats.NumberOfQueueOverflows += 1;

                                    StartNode.Stats.AddEventStat(
                                        State.CurrentDateTime,
                                        NodeStatistics.EventStatisticType.QueueOverflow,
                                        StartNode.Id);

                                }
                            }
                        }
                        else
                        {
                            foreach (var token in tokensToQueue)
                                StartNode.WaitingQueue.AddTokenToQueue(token);
                        }

                        //if (StartNode.IsStatsEnabled && StartNode.WaitingQueue.QueueLength > StartNode.Stats.MaximumTokensInQueue)
                        //    StartNode.Stats.MaximumTokensInQueue = StartNode.WaitingQueue.QueueLength;
                    }
                }

                bool continueProcessing = true;
                int changesThisIteration = 0;

                while (continueProcessing)
                {
                    // 2. Iterate through all tokens and take the relevant actions            
                    foreach (SimNodeBase node in Nodes)
                    {
                        // Ignore the node if there are no tokens in the waiting or execution queues
                        if (node.WaitingQueue.QueueLength > 0 || node.ExecutionQueue.QueueLength > 0)
                        {
                            // 2.1 Abandon tokens
                            var abandoningTokens = node.WaitingQueue.DequeueAbandoningTokens(_settings.TimeFactor);

                            if (abandoningTokens.Count() > 0 && node.IsStatsEnabled)
                            {
                                node.Stats.AddEventStat(
                                       State.CurrentDateTime,
                                       NodeStatistics.EventStatisticType.QueueAbandon,
                                       node.Id);
                            }

                            foreach (SimToken token in abandoningTokens)
                            {
                                token.Status = SimTokenStatus.Abandoned;
                                token.Stats.EndReason = TokenEndReason.Abandoned;
                                token.Stats.EndDate = State.CurrentDateTime;
                                //FlushLogsToStorage(token);
                                changesThisIteration++;
                            }

                            (CompletedTokens as List<ISimToken>).AddRange(abandoningTokens);

                            // Only ActivitySimNodes need to execute tokens
                            // Iterate through the tokens being executed, and determine if they are done
                            foreach (NodeQueueItem queueItem in node.ExecutionQueue)
                            {
                                if ((queueItem.CyclesInQueue * _settings.TimeFactor) >= node.ExecutionTime)
                                {
                                    // Token has been serviced long enough, move it to the next step
                                    SimNodeBase targetNode = null;

                                    switch (node)
                                    {
                                        case StartSimNode:
                                        case ActivitySimNode:
                                        case EventSimNode:
                                            targetNode = node.OutboundFlows.First().TargetNode as SimNodeBase;
                                            break;
                                        case GatewaySimNode g:
                                            if (g.OutboundFlows.Count() > 1)
                                                targetNode = g.SelectOutboundNode().TargetNode as SimNodeBase;
                                            else
                                                targetNode = g.OutboundFlows.First().TargetNode as SimNodeBase;
                                            break;
                                    }

                                    // sanity check
                                    ArgumentNullException.ThrowIfNull(targetNode);

                                    if ((targetNode is EventSimNode) && (targetNode as EventSimNode).Type == EventSimNodeType.End)
                                    {
                                        // token has reached the end event
                                        var completedToken = node.ExecutionQueue.DequeueToken(queueItem);
                                        (completedToken as SimToken).Stats.EndReason = TokenEndReason.Completed;
                                        (completedToken as SimToken).Stats.EndDate = State.CurrentDateTime;
                                        // record some stats here
                                        (CompletedTokens as List<ISimToken>).Add(completedToken);
                                    }
                                    else if (targetNode.WaitingQueue.SpaceInQueue > 0)
                                    {
                                        //There is space in the next node's waiting queue, so move this token there
                                        // First, update the statistics for this node
                                        if (node.IsStatsEnabled)
                                        {
                                            node.Stats.AddEventStat(
                                                   State.CurrentDateTime,
                                                   NodeStatistics.EventStatisticType.TokenLeftExecutionQueue,
                                                   node.Id);

                                            node.Stats.AddEventStat(
                                                   State.CurrentDateTime,
                                                   NodeStatistics.EventStatisticType.NodeExecuted,
                                                   node.Id);
                                        }

                                        var completedToken = node.ExecutionQueue.DequeueToken(queueItem);
                                        (completedToken as SimToken).Status = SimTokenStatus.InQueue;
                                        (completedToken as SimToken).CurrentLocation = targetNode;
                                        targetNode.WaitingQueue.AddTokenToQueue(completedToken);

                                        if (targetNode.IsStatsEnabled)
                                        {
                                            targetNode.Stats.AddEventStat(
                                                   State.CurrentDateTime,
                                                   NodeStatistics.EventStatisticType.TokenJoinedWaitingQueue,
                                                   targetNode.Id);
                                        }

                                        changesThisIteration++;
                                    }
                                    else
                                    {
                                        // No space in the next node's queue, so do nothing.
                                        // What's expected to happen is that the token stays in the ExecutionQueue of the
                                        // current node, and keeps incrementing its execution time
                                    }

                                }
                                else
                                {
                                    queueItem.CyclesInQueue++;
                                }
                            }


                            // Check if the node has enough resources to service another token
                            if (node.ExecutionQueue.SpaceInQueue > 0 && node.WaitingQueue.HasItemsInQueue)
                            {
                                // Move items from the WaitingQueue into the ServicingQueue
                                for (int i = 0; i < Math.Min(node.ExecutionQueue.SpaceInQueue, node.WaitingQueue.QueueLength); i++)
                                {
                                    var nextToken = node.WaitingQueue.DequeueTokenNextInLine();
                                    (nextToken as SimToken).Status = SimTokenStatus.Active;
                                    (nextToken as SimToken).CurrentLocation = node;
                                    node.ExecutionQueue.AddTokenToQueue(nextToken);
                                    if (node.IsStatsEnabled)
                                    {
                                        node.Stats.AddEventStat(
                                               State.CurrentDateTime,
                                               NodeStatistics.EventStatisticType.TokenLeftWaitingQueue,
                                               node.Id);

                                        node.Stats.AddEventStat(
                                               State.CurrentDateTime,
                                               NodeStatistics.EventStatisticType.TokenJoinedExecutionQueue,
                                               node.Id);
                                    }
                                    changesThisIteration++;
                                }
                            }
                        }
                    }

                    if (changesThisIteration == 0)
                        continueProcessing = false;
                    else
                        changesThisIteration = 0;

                }

                // 3. Determine if simulation should continue
                if (State.CurrentDateTime >= _settings.EndDateTime)
                {
                    continueExecution = false;
                }

                if (State.CurrentIteration >= _settings.MaximumIterations)
                {
                    continueExecution = false;
                }
            }

            FlushLogsToStorage();

            Trace.WriteLine("Simulation complete.");
        }

        /// <summary>
        /// Serialize this object.
        /// </summary>
        /// <returns>Serialized <see cref="Simulation"/>.</returns>
        public string Serialize()
        {
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };

            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Returns a textual representation of this <see cref="Simulation"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Simulation details: \n\r" +
                $"No of activity nodes: {Nodes.Count(x => x is ActivitySimNode)} \n\r" +
                $"No of gateway nodes: {Nodes.Count(x => x is GatewaySimNode)} \n\r" +
                $"No of event nodes: {Nodes.Count(x => x is EventSimNode)} \n\r";
        }

        public IEnumerable<EventStatistic> GetAllEventStatistics(IEnumerable<EventStatistic> stats)
        {
            // Nodes.Select(x => (x as SimNodeBase).ExecutionQueue.Select(y => y.TokenInQueue))
            // Nodes.Select(x => (x as SimNodeBase).WaitingQueue.Select(y => y.TokenInQueue))

            //var tokensInExecution = Nodes.Select(x => (x as SimNodeBase).ExecutionQueue.Select(y => y.TokenInQueue)).SelectMany(z => z);
            //var tokensInWaiting = Nodes.Select(x => (x as SimNodeBase).WaitingQueue.Select(y => y.TokenInQueue)).SelectMany(z => z);

            var returnVal = Nodes.Select(x => (x as SimNodeBase).Stats.EventStatistics).SelectMany(y => y);

            return returnVal;
        }

        #endregion

        #region Constructors

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

            CompletedTokens = new List<ISimToken>();
        }

        #endregion

        #region Private Methods

        private void FlushLogsToStorage()
        {
            foreach(var node in Nodes)
                FlushLogsToStorage(node);
        }

        private void FlushLogsToStorage(ISimNode node)
        {
            foreach (var nodeQueueItem in (node as SimNodeBase).WaitingQueue)
                FlushLogsToStorage(nodeQueueItem.TokenInQueue);

            foreach (var nodeQueueItem in (node as SimNodeBase).ExecutionQueue)
                FlushLogsToStorage(nodeQueueItem.TokenInQueue);
        }

        private void FlushLogsToStorage(ISimToken token)
        {

        }

        #endregion

        #region Fields

        private readonly ISimTokenGenerator _tokenGenerator;
        private readonly SimulationSettings _settings;
        private readonly Simulator _simulator;

        #endregion
    }

}
