using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illumine.LPR.DataModels
{
    [DisplayName("InfoText")]
    public class LPRInfoText
    {
        public string Temporary { get; set; }
        public string Vip { get; set; }
        public string NotVip { get; set; }
        public string NotOnPeriod { get; set; }
        public string Expired { get; set; }
        public string Incorrect { get; set; }
        public string SmartPay { get; set; }
        public string NoSpace { get; set; }
        public string NoPay { get; set; }
        public string NotCoherence { get; set; }
        public string Other { get; set; }
    }
}
