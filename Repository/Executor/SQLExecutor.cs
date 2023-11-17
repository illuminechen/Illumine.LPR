using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Illumine.LPR.Repository
{
    public class SQLExecutor : BaseExecutor
    {
        public string FilePath { get; }

        public string ConnectionString { get; set; }

        private string _SqlKey { get; }

        public object CreateSqlObj(SqlObjectHelper.ObjectKeyword keyword, params object[] parameters) => SqlObjectHelper.CreateSqlObj(this._SqlKey, keyword, parameters);

        public SQLExecutor(string path, string sqlKey, string connectionString)
          : base(path)
        {
            this.FilePath = path;
            this._SqlKey = sqlKey;
            this.ConnectionString = connectionString;
        }

        public override void Create()
        {
            DbConnection sqlObj = (DbConnection)CreateSqlObj(SqlObjectHelper.ObjectKeyword.Connection, (object)this.ConnectionString);
            if (sqlObj.State == ConnectionState.Open)
                sqlObj.Close();
            sqlObj.Open();
            sqlObj.Close();
        }

        public override void CreatePages(List<PageBase> pages)
        {
            List<string> cmds = new List<string>();
            pages.ForEach(page => cmds.Add(page.Create(RepoType.db).ToString()));
            this.ExecuteNonQueryWithTranscation(cmds.ToArray());
        }

        private DbConnection Open()
        {
            DbConnection sqlObj = (DbConnection)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Connection, (object)this.ConnectionString);
            if (sqlObj.State == ConnectionState.Open)
                sqlObj.Close();
            sqlObj.Open();
            return sqlObj;
        }

        public override void Delete<Data>(PageBase page, Data data) => this.ExecuteNonQueryWithTranscation((DbCommand)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Command, (object)string.Format("DELETE FROM [{0}] WHERE Id = {1}", (object)page.PageName, (object)data.Id)));

        public override void Insert<Data>(PageBase page, Data data)
        {
            DbCommand sqlObj1 = (DbCommand)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Command, (object)("INSERT INTO [" + page.PageName + "] (" + string.Join(",", ((IEnumerable<string>)page.FieldNames).Select<string, string>((Func<string, string>)(x => "[" + x + "]"))) + ") VALUES (" + string.Join(",", ((IEnumerable<string>)page.FieldNames).Select<string, string>((Func<string, string>)(x => "@" + x))) + ");"));
            foreach (string fieldName in page.FieldNames)
            {
                DbParameter sqlObj2 = (DbParameter)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Parameter, (object)(fieldName ?? ""), page.DataType.GetProperty(fieldName).GetValue((object)data));
                sqlObj1.Parameters.Add((object)sqlObj2);
            }
            this.ExecuteNonQueryWithTranscation(sqlObj1);
        }

        public override void Update<Data>(PageBase page, Data data)
        {
            DbCommand sqlObj1 = (DbCommand)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Command, (object)string.Format("UPDATE [{0}] SET {1} WHERE Id={2}", (object)page.PageName, (object)string.Join(",", ((IEnumerable<string>)page.FieldNames).Where<string>((Func<string, bool>)(x => x != "Id")).Select<string, string>((Func<string, string>)(x => "[" + x + "]=@" + x))), (object)data.Id));
            foreach (string name in ((IEnumerable<string>)page.FieldNames).Where<string>((Func<string, bool>)(x => x != "Id")))
            {
                DbParameter sqlObj2 = (DbParameter)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Parameter, (object)(name ?? ""), page.DataType.GetProperty(name).GetValue((object)data));
                sqlObj1.Parameters.Add((object)sqlObj2);
            }
            this.ExecuteNonQueryWithTranscation(sqlObj1);
        }

        private DataTable Query(string cmdString)
        {
            DataSet dataSet1 = new DataSet();
            using (DbConnection dbConnection = Open())
            {
                using (DataAdapter sqlObj = (DataAdapter)CreateSqlObj(SqlObjectHelper.ObjectKeyword.DataAdapter, cmdString, dbConnection))
                {
                    sqlObj.Fill(dataSet1);
                }
            }
            return dataSet1.Tables[0];
        }

        private void ExecuteNonQueryWithTranscation(params DbCommand[] cmds)
        {
            DbConnection dbConnection = this.Open();
            DbTransaction dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach (DbCommand cmd in cmds)
                {
                    cmd.Connection = dbConnection;
                    cmd.Transaction = dbTransaction;
                    cmd.ExecuteNonQuery();
                }
                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw ex;
            }
            if (dbConnection.State != ConnectionState.Open)
                return;
            dbConnection.Close();
        }

        private void ExecuteNonQueryWithTranscation(params string[] cmdStrs)
        {
            DbConnection dbConnection = this.Open();
            DbTransaction dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach (string cmdStr in cmdStrs)
                    ((DbCommand)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Command, (object)cmdStr, (object)dbConnection, (object)dbTransaction)).ExecuteNonQuery();
                dbTransaction.Commit();
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                throw ex;
            }
            if (dbConnection.State != ConnectionState.Open)
                return;
            dbConnection.Close();
        }

        public override List<Data> Read<Data>(PageBase page, int limit = -1, SortOrder order = SortOrder.Unspecified)
        {
            string str = limit == -1 ? "" : string.Format("LIMIT {0}", limit);
            string cmdString;
            if (order == SortOrder.Descending && limit != -1)
                cmdString = "SELECT * FROM (SELECT * FROM [" + page.PageName + "] " + order.ToReverseSqlCmd() + " ) " + order.ToSqlCmd() + " " + str;
            else
                cmdString = "SELECT " + str + " * FROM [" + page.PageName + "] " + order.ToSqlCmd() + " " + str;

            return Query(cmdString).Rows.Cast<DataRow>().Select(dr => ItemConverter<Data>.GetData(dr)).ToList();
        }

        public override void ResetId<Data>(PageBase page, Data data, int newId)
        {
            DbCommand sqlObj1 = (DbCommand)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Command, (object)string.Format("UPDATE [{0}] SET Id=@Id WHERE Id={1}", (object)page.PageName, (object)data.Id));
            DbParameter sqlObj2 = (DbParameter)this.CreateSqlObj(SqlObjectHelper.ObjectKeyword.Parameter, (object)"@Id", (object)newId);
            sqlObj1.Parameters.Add((object)sqlObj2);
            this.ExecuteNonQueryWithTranscation(sqlObj1);
        }

        public override bool TryGetFieldNames(PageBase page, out string[] FieldNames)
        {
            FieldNames = new string[] { };
            try
            {
                string cmdString = "SELECT * FROM [" + page.PageName + "] LIMIT 1";
                FieldNames = Query(cmdString).Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }
}
