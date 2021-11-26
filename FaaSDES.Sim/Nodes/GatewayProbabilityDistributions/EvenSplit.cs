using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes.GatewayProbabilityDistributions
{
    public class EvenSplit : IGatewayProbabilityDistribution
    {       

        public SequenceFlow ChooseSequenceFlow(IEnumerable<SequenceFlow> sequenceFlows)
        {
            throw new NotImplementedException();
        }

        public double Generate()
        {
            var leastOption = _selectionHistory.MinBy(x => x.Value);
            return leastOption.Key;
        }

        public EvenSplit(int numberOfOptions)
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                _selectionHistory.Add(i, 0);
            }
        }

        private Dictionary<int, int> _selectionHistory = new();
    }
}
