using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.UI.Xaml.Diagram.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.UI.Nodes
{
    public class FaaSDESEventNodeViewModel : FaaSDESNodeViewModel
    {        

        [Display(Name = "Event Type")]
        [Category("Simulation Parameters - Events")]
        public EventType SimulationEventType
        {
            get
            {
                return this.EventType;
            }
        }

        [Display(Name = "Event Trigger")]
        [Category("Simulation Parameters - Events")]
        public EventTrigger SimulationEventTrigger
        {
            get
            {
                return this.EventTrigger;
            }
        }
    }
}
