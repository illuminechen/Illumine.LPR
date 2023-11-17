using System;
using System.IO;

namespace Illumine.LPR
{
    public static class RepoTypeHelper
    {
        public static RepoType GetRepoType(string fileName)
        {
            RepoType result;
            return !Enum.TryParse<RepoType>(Path.GetExtension(fileName).TrimStart('.'), out result) ? RepoType.unknow : result;
        }
    }
}
