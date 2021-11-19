using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.UI.Xaml.Diagram.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.UI.Nodes
{
    public class FaaSDESActivityNodeViewModel : FaaSDESNodeViewModel
    {
        [Display(Name = "Execution Time (sec)")]
        [Category("Simulation Parameters - Activities")]
        public int ExecutionTimeInSeconds { get; set; } = 0;

        [Display(Name = "Activity")]
        [Category("Simulation Parameters - Activities")]
        public string ActivityContent
        {
            get
            {
                if (this.Annotations != null && (this.Annotations as ObservableCollection<IAnnotation>).First().Content != null)
                    return (this.Annotations as ObservableCollection<IAnnotation>).First().Content.ToString();
                else
                    return string.Empty;
            }
        }
    }
}
