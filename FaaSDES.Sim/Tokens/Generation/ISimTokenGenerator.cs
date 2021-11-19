using System.Collections.Generic;

namespace FaaSDES.Sim.Tokens.Generation
{
    public interface ISimTokenGenerator
    {
        public IEnumerable<SimToken> GetNextTokens(SimulationState state);

        public void Stop();

        public void Start();
    }
}