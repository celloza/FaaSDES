using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public abstract class SimNodeBase : ISimNode
    {
        public int MaxQueueLength { get; set; }



        public void PurgeQueue()
        {
            throw new NotImplementedException();
        }
    }
}
