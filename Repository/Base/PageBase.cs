using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Illumine.LPR.Repository
{
    public abstract class PageBase
    {
        public Type DataType { get; }

        public string PageName { get; }

        public string[] FieldNames => ColumnDict.Keys.ToArray();

        public Dictionary<string, string> ColumnDict { get; }

        public RepositoryBase Repository { get; }

        public PageBase(
          RepositoryBase repository,
          string pageName,
          Dictionary<string, string> columnDict,
          Type dataType)
        {
            this.PageName = pageName;
            this.ColumnDict = columnDict;
            this.Repository = repository;
            this.DataType = dataType;
        }

        public virtual object Create(RepoType repoType = RepoType.unknow)
        {
            if (repoType == RepoType.xlsx)
                return $"CREATE TABLE [{PageName}] ({string.Join(",", FieldNames.Select(x => "[" + x + "] VARCHAR"))});";
            return repoType == RepoType.db ? $"CREATE TABLE IF NOT EXISTS [{PageName}] ({string.Join(",", ColumnDict.Select(kv => "[" + kv.Key + "] " + kv.Value))});" : "";
        }

        public virtual bool CheckFormat()
        {
            if (!this.Repository.Executor.TryGetFieldNames(this, out var fieldNames))
            {
                throw new Exception("Cannot get field names");
            }

            return FieldNames.SequenceEqual(fieldNames);
        }

        public virtual List<object> ReadList(int limit = -1, SortOrder order = SortOrder.Unspecified)
        {
            MethodInfo methodInfo = typeof(PageBase<>).MakeGenericType(DataType).GetMethod("Read");
            IList instance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(DataType), methodInfo.Invoke(this, new object[2]
            {
                limit,
                order
            }));
            List<object> objectList = new List<object>();
            foreach (object obj in instance)
                objectList.Add(obj);
            return objectList;
        }

        public virtual void InsertData(object data)
        {
            MethodInfo methodInfo = typeof(PageBase<>).MakeGenericType(DataType).GetMethod("Insert");
            methodInfo.Invoke(this, new object[] { data });
        }

    }

    public abstract class PageBase<Data> : PageBase where Data : class, IIndexData, new()
    {
        protected PageBase(RepositoryBase repository, string pageName, Dictionary<string, string> columnDict)
          : base(repository, pageName, columnDict, typeof(Data))
        {
        }

        public virtual void Delete(Data data) => this.Repository.Executor.Delete(this, data);

        public virtual void Insert(Data data) => this.Repository.Executor.Insert(this, data);

        public virtual List<Data> Read(int limit = -1, SortOrder order = SortOrder.Unspecified) => this.Repository.Executor.Read<Data>(this, limit, order);

        public virtual void Update(Data data) => this.Repository.Executor.Update(this, data);

    }
}
