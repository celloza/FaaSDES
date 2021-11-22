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
        public IEnumerable<ISimNode> Nodes { get; set; }

        public StartSimNode StartNode { get; internal set; }

        public SimulationState State { get; set; }

        public void Execute()
        {
            // On each simulation cycle:
            //
            // 0. Update the SimulationState
            // 1. Generate the requisite tokens using the token generator
            // 2. Iterate through all tokens, and either:
            // 2.1 Stay where it is
            // 2.2 Move to the next node
            // 2.3 Abandon (TBC)
            // 3. Determine if simulation should continue

            // 0. Update the SimulationState
            State.CurrentIteration++;
            State.CurrentDateTime += _settings.Increment;

            // 1. Generate the requisite tokens using the token generator
            if (StartNode.TokenQueue.SpaceInQueue > 0)
            {
                var tokensToQueue = _tokenGenerator.GetNextTokens(State);
                var placesInQueue = StartNode.TokenQueue.SpaceInQueue;
                
                if (tokensToQueue.Count() > placesInQueue)
                {
                    var tokensToAdd = tokensToQueue.Take(placesInQueue);
                    foreach (var token in tokensToAdd)
                    {
                        StartNode.TokenQueue.AddTokenToQueue(token);
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
                        StartNode.TokenQueue.AddTokenToQueue(token);
                }

                if (StartNode.IsStatsEnabled && StartNode.TokenQueue.QueueLength > StartNode.Stats.MaximumTokensInQueue)
                    StartNode.Stats.MaximumTokensInQueue = StartNode.TokenQueue.QueueLength;
            }

            // 2. Iterate through all tokens and take the relevant actions            
            foreach(SimNodeBase node in Nodes)
            {



                // 2.3 Abandon tokens
                var abandoningTokens = node.TokenQueue.DequeueAbandoningTokens(State.CurrentIteration);
                if(node.IsStatsEnabled)
                    node.Stats.NumberOfQueueAbandons += abandoningTokens.Count();
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
