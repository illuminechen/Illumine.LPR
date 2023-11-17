using System;

namespace Illumine.LPR.Repository
{
    public static class ExecutorFactory
    {
        public static BaseExecutor Create(string fileName)
        {
            RepoType repoType = RepoTypeHelper.GetRepoType(fileName);
            return ExecutorFactory.Create(fileName, repoType);
        }

        public static BaseExecutor Create(string fileName, RepoType repoType)
        {
            switch (repoType)
            {
                case RepoType.csv:
                    return (BaseExecutor)ExecutorFactory.GetExecutor<CSVExecutor>(fileName, ",");
                case RepoType.xml:
                    return (BaseExecutor)ExecutorFactory.GetExecutor<XMLExecutor>(fileName);
                case RepoType.xlsx:
                    return (BaseExecutor)ExecutorFactory.GetExecutor<ExcelExecutor>(fileName, "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0 Xml;HDR=Yes;\"");
                case RepoType.db:
                    return (BaseExecutor)ExecutorFactory.GetExecutor<SQLExecutor>(fileName, "System.Data.SQLite.SQLite{0}, System.Data.SQLite", "Data Source = " + fileName + "; Version = 3; New = False; Compress = True;");
                default:
                    return (BaseExecutor)ExecutorFactory.GetExecutor<CSVExecutor>(fileName, "\t");
            }
        }

        private static T GetExecutor<T>(params string[] param)
        {
            string Key = param[0];
            T instance = Container.Get<string, T>(Key);
            if ((object)instance == null)
            {
                instance = (T)Activator.CreateInstance(typeof(T), (object[])param);
                Container.Put<string, T>(Key, instance);
            }
            return instance;
        }
    }
}
