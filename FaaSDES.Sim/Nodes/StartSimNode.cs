using FaaSDES.Sim.Tokens.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class StartSimNode : SimNodeBase
    {
        public ISimTokenGenerator TokenGenerator { get; }

        public StartSimNode(Simulation simulation, ISimTokenGenerator generator, string id, string name)
            : base(simulation, id, name)
        {
            TokenGenerator = generator;
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
        }
    }
}
