using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Tokens.Generation
{
    public abstract class SimTokenGeneratorBase : ISimTokenGenerator
    {        

        public bool IsGeneratingTokens
        {
            get { return _isGeneratingTokens; }
            set { _isGeneratingTokens = value; }
        }

        public abstract IEnumerable<SimToken> GetNextTokens(SimulationState state);

        public abstract override string ToString();
        

        public void Start()
        {
            _isGeneratingTokens = true;
        }

        public void Stop()
        {
            _isGeneratingTokens = false;
        }

        private bool _isGeneratingTokens;
    }
}
