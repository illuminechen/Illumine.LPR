using System;

namespace Illumine.LPR
{
    public class KeyValuePairViewModel : BaseViewModel
    {
        public bool Visible { get; set; } = true;

        public string KeyText { get; set; }

        public string ValueText { get; set; }
    }
}
