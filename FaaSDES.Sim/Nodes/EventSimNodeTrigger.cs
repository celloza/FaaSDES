using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    public enum EventSimNodeTrigger
    {
        None,
        Message,
        Timer,
        Conditional,
        Link,
        Signal,
        Error,
        Escalation,
        Termination,
        Compensation,
        Cancel,
        Multiple,
        MutlipleParallel
            
    }
}
