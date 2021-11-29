using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Tokens
{
    public class SimTokenStats
    {
        public TimeSpan TotalWaitTime { get; set; }

        public TimeSpan TotalExecutionTime { get; set; }

        public DateTime GenerationDate { get; set; }

        public DateTime EndDate { get; set; }

        public TokenEndReason EndReason { get; set; }


    }
}
