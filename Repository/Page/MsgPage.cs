using System.Collections.Generic;
using System.Linq;

namespace Illumine.LPR.Repository
{
    public class MsgPage : PageBase<MsgData>
    {
        const string TABLENAME = "Msg";
        static Dictionary<string, string> COLUMNS = new Dictionary<string, string>
        {
            { "Id", "INTEGER PRIMARY KEY AUTOINCREMENT"},
            { "TimeStamp", "DATE"},
            { "ChannelId", "INTEGER"},
            { "ImagePath", "TEXT"},
            { "PlateX","INTEGER" },
            { "PlateY","INTEGER" },
            { "PlateWidth", "INTEGER" },
            { "PlateHeight", "INTEGER" },
            { "PlateNumber", "TEXT(10)"},
            { "ParkingMode", "INTEGER"},
            { "Tag", "TEXT"},
        };
        public MsgPage(RepositoryBase repository)
          : base(repository, TABLENAME, COLUMNS
          )
        {
        }
    }
}
