using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;

namespace Illumine.LPR.Repository
{
    public class ExcelExecutor : BaseExecutor
    {
        public string FilePath { get; }

        public string ConnectionString { get; set; }

        public ExcelExecutor(string path, string connectionString)
          : base(path)
        {
            this.FilePath = path;
            this.ConnectionString = connectionString;
        }

        public override void Create()
        {
            OleDbConnection oleDbConnection = new OleDbConnection(this.ConnectionString);
            if (oleDbConnection.State == ConnectionState.Open)
                oleDbConnection.Close();
            oleDbConnection.Open();
            oleDbConnection.Close();
        }

        public override void CreatePages(List<PageBase> pages)
        {
            List<string> cmds = new List<string>();
            pages.ForEach((Action<PageBase>)(page => cmds.Add(page.Create(RepoType.xlsx).ToString())));
            this.ExecuteNonQueryWithTranscation(cmds.ToArray());
        }

        private OleDbConnection Open()
        {
            OleDbConnection oleDbConnection = new OleDbConnection(this.ConnectionString);
            if (oleDbConnection.State == ConnectionState.Open)
                oleDbConnection.Close();
            oleDbConnection.Open();
            return oleDbConnection;
        }

        public override void Delete<Data>(PageBase page, Data data) => this.ExecuteNonQueryWithTranscation(new OleDbCommand(string.Format("DELETE FROM [{0}$] WHERE Id = \"{1}\"", (object)page.PageName, (object)data.Id)));

        public override void Insert<Data>(PageBase page, Data data)
        {
            OleDbCommand oleDbCommand = new OleDbCommand("INSERT INTO [" + page.PageName + "$] (" + string.Join(",", ((IEnumerable<string>)page.FieldNames).Select<string, string>((Func<string, string>)(x => "[" + x + "]"))) + ") VALUES (" + string.Join(",", ((IEnumerable<string>)page.FieldNames).Select<string, string>((Func<string, string>)(x => "?"))) + ");");
            foreach (string fieldName in page.FieldNames)
            {
                PropertyInfo property = page.DataType.GetProperty(fieldName);
                oleDbCommand.Parameters.Add(new OleDbParameter(fieldName ?? "", property.GetValue(data) ?? DBNull.Value)
                {
                    OleDbType = OleDbType.VarChar
                });
            }
            this.ExecuteNonQueryWithTranscation(oleDbCommand);
        }

        public override void Update<Data>(PageBase page, Data data)
        {
            OleDbCommand oleDbCommand = new OleDbCommand(string.Format("UPDATE [{0}$] SET {1} WHERE Id=\"{2}\"", page.PageName, arg1: (object)string.Join(",", ((IEnumerable<string>)page.FieldNames).Where<string>((Func<string, bool>)(x => x != "Id")).Select<string, string>((Func<string, string>)(x => "[" + x + "]=?"))), (object)data.Id));
            foreach (string name in ((IEnumerable<string>)page.FieldNames).Where(x => x != "Id"))
            {
                PropertyInfo property = page.DataType.GetProperty(name);
                oleDbCommand.Parameters.Add(new OleDbParameter(name ?? "", property.GetValue(data) ?? DBNull.Value)
                {
                    OleDbType = OleDbType.VarChar
                });
            }
            this.ExecuteNonQueryWithTranscation(oleDbCommand);
        }

        private DataTable Query(string cmdString)
        {
            OleDbConnection selectConnection = this.Open();
            OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(cmdString, selectConnection);
            DataSet dataSet1 = new DataSet();
            oleDbDataAdapter.Fill(dataSet1);
            if (selectConnection.State == ConnectionState.Open)
                selectConnection.Close();
            return dataSet1.Tables[0];
        }

        private void ExecuteNonQueryWithTranscation(params OleDbCommand[] cmds)
        {
            OleDbConnection oleDbConnection = this.Open();
            OleDbTransaction oleDbTransaction = oleDbConnection.BeginTransaction();
            try
            {
                foreach (OleDbCommand cmd in cmds)
                {
                    cmd.Connection = oleDbConnection;
                    cmd.Transaction = oleDbTransaction;
                    cmd.ExecuteNonQuery();
                }
                oleDbTransaction.Commit();
            }
            catch (Exception ex)
            {
                oleDbTransaction.Rollback();
                throw ex;
            }
            if (oleDbConnection.State != ConnectionState.Open)
                return;
            oleDbConnection.Close();
        }

        private void ExecuteNonQueryWithTranscation(params string[] cmdStrs)
        {
            OleDbConnection connection = this.Open();
            OleDbTransaction transaction = connection.BeginTransaction();
            try
            {
                foreach (string cmdStr in cmdStrs)
                    new OleDbCommand(cmdStr, connection, transaction).ExecuteNonQuery();
                transaction.Commit();
            }
            catch (OleDbException ex)
            {
                transaction.Rollback();
                if (ex.ErrorCode != 3010)
                {
                    if (ex.ErrorCode != 3012)
                        throw ex;
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            if (connection.State != ConnectionState.Open)
                return;
            connection.Close();
        }

        public override List<Data> Read<Data>(PageBase page, int limit = -1, SortOrder order = SortOrder.Unspecified)
        {
            List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
            List<Data> list = this.Query("SELECT * FROM [" + page.PageName + "$] Where [Id] IS NOT NULL ").Rows.Cast<DataRow>().Select(dr => ItemConverter<Data>.GetData(dr)).ToList<Data>();
            switch (order)
            {
                case SortOrder.Ascending:
                    list.Sort((x, y) => x.Id <= y.Id ? -1 : 1);
                    break;
                case SortOrder.Descending:
                    list.Sort((x, y) => x.Id <= y.Id ? 1 : -1);
                    break;
            }
            if (limit != -1 && list.Count > limit)
            {
                list = list.GetRange(0, limit);
            }
            return list;
        }

        public override void ResetId<Data>(PageBase page, Data data, int newId) => this.ExecuteNonQueryWithTranscation(new OleDbCommand(string.Format("UPDATE [{0}$] SET Id=? WHERE Id=\"{1}\"", (object)page.PageName, (object)data.Id))
        {
            Parameters = {
                new OleDbParameter("@Id",  newId.ToString())
                {
                  OleDbType = OleDbType.VarChar
                }
              }
        });

        public override bool TryGetFieldNames(PageBase page, out string[] FieldNames)
        {
            FieldNames = new string[] { };
            try
            {
                FieldNames = Query("SELECT * FROM [" + page.PageName + "$]").Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
