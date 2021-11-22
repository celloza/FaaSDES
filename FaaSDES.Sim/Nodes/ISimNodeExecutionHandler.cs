using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public interface ISimNodeExecutionHandler
    {
        void Execute(ISimNode node);
    }
}
