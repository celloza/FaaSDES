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
            throw new NotImplementedException();
        }

        public EvenSplit(int numberOfOptions)
        {
            _selectionHistory = new int[numberOfOptions, 2];

            for (int i = 0; i < numberOfOptions; i++)
            {
                _selectionHistory[i, 0] = i;
                _selectionHistory[i, 1] = 0;
            }
        }

        private int[,] _selectionHistory;
    }
}
