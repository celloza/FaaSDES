namespace FaaSDES.Sim.Nodes.GatewayProbabilityDistributions
{
    /// <summary>
    /// An implementation of <see cref="IGatewayProbabilityDistribution"/> that 
    /// randomly picks amongst the provided options.
    /// </summary>
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

        private readonly System.Random _random = new();
    }
}
