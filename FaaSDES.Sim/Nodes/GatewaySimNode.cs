using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public class GatewaySimNode : SimNodeBase
    {
        public GatewaySimNode(string id, string name,
                IEnumerable<SequenceFlow> inboundFlows, IEnumerable<SequenceFlow> outboundFlows)
            : base(id, name, inboundFlows, outboundFlows)
        {

        }

        public GatewaySimNode(string id, string name)
           : base(id, name, null, null)
        {
            OutboundFlows = new List<SequenceFlow>();
            InboundFlows = new List<SequenceFlow>();
        }

        public GatewaySimNode(Simulation simulation, string id, string name)
            : base(simulation, id, name)
        {

        }
    }
}
