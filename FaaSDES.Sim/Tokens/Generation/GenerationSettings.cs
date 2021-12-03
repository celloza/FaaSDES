using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Tokens.Generation
{
    /// <summary>
    /// Settings for <see cref="ISimTokenGenerator"/>s.
    /// </summary>
    public class GenerationSettings
    {
        /// <summary>
        /// The probabilistic distribution that a generator should produce a
        /// token.
        /// </summary>
        public object ProbabilisticDistribution { get; set; }

        public int MinimumTokensPerGeneration { get; set; }

        public int MaximumTokensPerGeneration { get; set; }

        public GenerationSettings(int minimumTokensPerGeneration, int maximumTokensPerGeneration)
        {
            MinimumTokensPerGeneration = minimumTokensPerGeneration;
            MaximumTokensPerGeneration = maximumTokensPerGeneration;
        }

        public override string ToString()
        {
            //TODO: return actual name of distribution
            return $"Token generator settings: \n\r" +
                $"Minimum tokens to generate per generation: {MinimumTokensPerGeneration} \n\r" +
                $"Maximum tokens to generate per generation: {MaximumTokensPerGeneration} \n\r" +
                $"Probabalistic distribution method: Random";
        }

    }
}
