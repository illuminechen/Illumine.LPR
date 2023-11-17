namespace Illumine.LPR.Repository
{
    public static class SortOrderExtension
    {
        public static string ToSqlCmd(this SortOrder order)
        {
            switch (order)
            {
                case SortOrder.Ascending:
                    return "ORDER BY Id ASC";
                case SortOrder.Descending:
                    return "ORDER BY Id DESC";
                default:
                    return "";
            }
        }

        public static string ToReverseSqlCmd(this SortOrder order)
        {
            switch (order)
            {
                case SortOrder.Ascending:
                    return "ORDER BY Id DESC";
                case SortOrder.Descending:
                    return "ORDER BY Id ASC";
                default:
                    return "";
            }
        }
    }
}
