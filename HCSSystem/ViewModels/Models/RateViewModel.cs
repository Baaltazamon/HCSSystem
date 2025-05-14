using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSSystem.ViewModels.Models
{
    public class RateViewModel
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal PricePerUnit { get; set; }
        public DateTime EffectiveFrom { get; set; }
    }
}
