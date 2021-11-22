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
            if (StartNode.CanEnqueueToken)
            {
                foreach (ISimToken token in _tokenGenerator.GetNextTokens(State))
                    StartNode.Tokens.Enqueue(token);
            }

            // 2. Iterate through all tokens and take the relevant actions




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
