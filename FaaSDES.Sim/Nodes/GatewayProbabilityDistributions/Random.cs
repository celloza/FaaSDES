using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes.GatewayProbabilityDistributions
{
    public class Random : IGatewayProbabilityDistribution
    {
        public SequenceFlow ChooseSequenceFlow(IEnumerable<SequenceFlow> sequenceFlows)
        {
            return sequenceFlows.ElementAt(_random.Next(1, sequenceFlows.Count()));
        }

        public double Generate()
        {
            return _random.NextDouble();
        }

        private System.Random _random = new();
    }
}
