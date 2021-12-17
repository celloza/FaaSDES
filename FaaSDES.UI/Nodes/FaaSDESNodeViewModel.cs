using FaaSDES.Sim.Nodes;
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
    public abstract class FaaSDESNodeViewModel : BpmnNodeViewModel, IBpmnNode
    {
        [Display(Name = "Node Type")]
        [Category("Simulation Parameters")]
        public FaaSDESActivityNodeType NodeType
        {
            get
            {
                switch (this.Type)
                {
                    case Syncfusion.UI.Xaml.Diagram.Controls.BpmnShapeType.Event:
                        if (this.EventType == Syncfusion.UI.Xaml.Diagram.Controls.EventType.Start)
                            return FaaSDESActivityNodeType.EventStart;
                        else
                            return FaaSDESActivityNodeType.EventEnd;
                    case Syncfusion.UI.Xaml.Diagram.Controls.BpmnShapeType.Activity:
                        return FaaSDESActivityNodeType.Activity;
                    case Syncfusion.UI.Xaml.Diagram.Controls.BpmnShapeType.Gateway:
                        return FaaSDESActivityNodeType.Gateway;
                    default:
                        return FaaSDESActivityNodeType.Undefined;
                }
            }
        }

        public ISimNode FaaSDESNode { get; set; }
    }
}
