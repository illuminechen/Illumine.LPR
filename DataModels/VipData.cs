using System;
using System.Linq;

namespace Illumine.LPR
{
    public class VipData : IIndexData
    {
        public int Id { get; set; }

        public int? Group { get; set; }

        public string Name { get; set; }

        public string PlateNumber { get; set; }

        public string ETagNumber { get; set; }

        public DateTime ExpireTime { get; set; } = DateTime.Now;

        public string Periods { get; set; } = string.Join("",Enumerable.Repeat("1",24));
        
        public string Description { get; set; }
    }
}
