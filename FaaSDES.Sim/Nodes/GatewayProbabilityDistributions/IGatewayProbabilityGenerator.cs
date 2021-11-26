using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes.GatewayProbabilityDistributions
{
    public interface IGatewayProbabilityDistribution
    {

        public SequenceFlow ChooseSequenceFlow(IEnumerable<SequenceFlow> sequenceFlows);

        /// <summary>
        /// Generates a random number using the probability generator.
        /// </summary>
        /// <returns></returns>
        public double Generate();
    }
}
