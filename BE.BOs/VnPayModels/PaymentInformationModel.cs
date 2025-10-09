using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.BOs.VnPayModels
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
