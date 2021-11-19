using Syncfusion.UI.Xaml.Diagram;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.UI.Nodes
{
    public class FaaSDESFlowViewModel : BpmnFlowViewModel
    {
        [Display(Name = "Max Queue Length")]
        [Category("Simulation Parameters")]
        public int MaxQueueLength { get; set; } = 0;
    }
}
