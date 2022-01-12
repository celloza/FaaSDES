namespace FaaSDES.Sim.Nodes.GatewayProbabilityDistributions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGatewayProbabilityDistribution
    {
        public SequenceFlow ChooseSequenceFlow(IEnumerable<SequenceFlow> sequenceFlows);

        /// <summary>
        /// Generates a random number using the implemented probability generator.
        /// </summary>
        /// <returns></returns>
        public double Generate();
    }
}
