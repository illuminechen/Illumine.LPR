using System;

namespace Illumine.LPR
{
    public class GroupData : IIndexData
    {
        public int Id { get; set; }

        public string GroupName { get; set; }

        public int TotalCount { get; set; }

        public int CurrentCount { get; set; }

    }
}
