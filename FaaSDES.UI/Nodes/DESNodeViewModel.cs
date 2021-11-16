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
    public class DESNodeViewModel : BpmnNodeViewModel, IBpmnNode
    {
        [Display(Name = "Execution Time (sec)")]
        [Category("Simulation Parameters")]
        public int ExecutionTimeInSeconds { get; set; } = 0;

        [Display(Name = "Node Type")]
        [Category("Simulation Parameters")]
        public DESNodeType NodeType
        {
            get
            {
                switch(this.Type)
                {
                    case Syncfusion.UI.Xaml.Diagram.Controls.BpmnShapeType.Event:
                        if (this.EventType == Syncfusion.UI.Xaml.Diagram.Controls.EventType.Start)
                            return DESNodeType.EventStart;
                        else
                            return DESNodeType.EventEnd;
                    case Syncfusion.UI.Xaml.Diagram.Controls.BpmnShapeType.Activity:
                        return DESNodeType.Activity;
                    case Syncfusion.UI.Xaml.Diagram.Controls.BpmnShapeType.Gateway:
                        return DESNodeType.Gateway;
                    default:
                        return DESNodeType.Undefined;
                }               
            }
        }        
    }

    public enum DESNodeType
    {
        EventStart,
        EventEnd,
        Gateway,
        Activity,
        Undefined

    }
}
