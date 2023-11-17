using System.Collections.Generic;
using System.Linq;

namespace Illumine.LPR.Repository
{
    public class GroupPage : PageBase<GroupData>
    {
        const string TABLENAME = "Group";
        static Dictionary<string, string> COLUMNS = new Dictionary<string, string>
        {
            { "Id", "INTEGER PRIMARY KEY"},
            { "GroupName", "TEXT"},
            { "TotalCount", "INTEGER"},
            { "CurrentCount", "INTEGER"},
        };

        public GroupPage(RepositoryBase repository)
          : base(repository, TABLENAME, COLUMNS)
        {
        }

        public void ResetId(GroupData data, int newId) => this.Repository.Executor.ResetId(this, data, newId);
    }
}
