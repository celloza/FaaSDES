using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.UI.Nodes
{
    public class FaaSDESGatewayViewModel : FaaSDESNodeViewModel
    {
        [Display(Name = "Distribution Function")]
        [Category("Simulation Parameters - Gateways")]
        public DistributionFunctions DistributionFunction { get; set; } = 0;
    }

    public enum DistributionFunctions
    {
        Even,
        Normal,
        Poisson,
        Exponential
    }
}
