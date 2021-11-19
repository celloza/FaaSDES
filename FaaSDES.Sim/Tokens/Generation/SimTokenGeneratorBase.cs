using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Tokens.Generation
{
    public abstract class SimTokenGeneratorBase : ISimTokenGenerator
    {
        private bool isGeneratingTokens;

        public bool IsGeneratingTokens
        {
            get { return isGeneratingTokens; }
            set { isGeneratingTokens = value; }
        }

        public abstract IEnumerable<SimToken> GetNextTokens(SimulationState state);

        public void Start()
        {
            isGeneratingTokens = true;
        }

        public void Stop()
        {
            isGeneratingTokens = false;
        }
    }
}
