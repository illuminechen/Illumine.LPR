using System.Collections.Generic;
using System.Linq;

namespace Illumine.LPR.Repository
{
    public class VipPage : PageBase<VipData>
    {
        const string TABLENAME = "Vip";
        static Dictionary<string, string> COLUMNS = new Dictionary<string, string>
        {
            { "Id", "INTEGER PRIMARY KEY" },
            { "Group", "INTEGER" },
            { "Name", "TEXT" },
            { "PlateNumber", "TEXT(10)" },
            { "ETagNumber", "TEXT(50)" },
            { "ExpireTime", "DATE" },
            { "Periods", "TEXT" },
            { "Description", "TEXT" },
        };
        public VipPage(RepositoryBase repository)
          : base(repository, TABLENAME, COLUMNS)
        {
        }

        public void ResetId(VipData data, int newId) => this.Repository.Executor.ResetId(this, data, newId);
    }
}
