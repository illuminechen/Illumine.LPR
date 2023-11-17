using Illumine.LPR.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Illumine.LPR
{
    public static class RepositoryService
    {
        public static List<RepositoryBase> Repositories = new List<RepositoryBase>() { MsgRepository.Instance, DataRepository.Instance };

        public static void UpdateSchema()
        {
            foreach (var repo in Repositories)
            {
                try
                {
                    if (repo.Pages.Exists(x => !x.CheckFormat()))
                    {
                        var dr = System.Windows.Forms.MessageBox.Show($"{Path.GetFileName(repo.FileName)}格式不一致，是否嘗試更新格式？", "更新檔案格式", System.Windows.Forms.MessageBoxButtons.YesNo);
                        if (dr == System.Windows.Forms.DialogResult.Yes)
                        {
                            repo.ReCreate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    repo.Create();
                }
            }
        }

        #region Data

        public static void OutputData(string targetPath, RepoType repoType) => DataRepository.Instance.Export(targetPath, repoType);
        public static void InputData(string sourcePath, RepoType repoType)
        {
            DataRepository.Instance.Import(sourcePath, repoType);
        }

        #region Vip
        public static void Insert(VipData data) => DataRepository.Instance.Vip.Insert(data);

        public static void Update(VipData data) => DataRepository.Instance.Vip.Update(data);

        public static void Delete(VipData data) => DataRepository.Instance.Vip.Delete(data);

        public static List<VipData> GetVip() => DataRepository.Instance.Vip.Read(order: SortOrder.Ascending);

        public static void ResetId(VipData data, int newId) => DataRepository.Instance.Vip.ResetId(data, newId);
        #endregion

        #region Group
        public static void Insert(GroupData data) => DataRepository.Instance.Group.Insert(data);
        public static void Update(GroupData data) => DataRepository.Instance.Group.Update(data);
        public static void Delete(GroupData data) => DataRepository.Instance.Group.Delete(data);
        public static List<GroupData> GetGroup() => DataRepository.Instance.Group.Read(order: SortOrder.Ascending);
        public static void ResetId(GroupData data, int newId) => DataRepository.Instance.Group.ResetId(data, newId);
        #endregion

        #endregion


        #region Msg
        public static void OutputMsg(string targetPath, RepoType repoType) => MsgRepository.Instance.Export(targetPath, repoType);
        public static void IntputMsg(string sourcePath, RepoType repoType) => MsgRepository.Instance.Import(sourcePath, repoType);

        #region Msg
        public static void Insert(MsgData msg) => MsgRepository.Instance.Msg.Insert(msg);
        public static void Update(MsgData msg) => MsgRepository.Instance.Msg.Update(msg);
        public static List<MsgData> GetMsg() => MsgRepository.Instance.Msg.Read(order: SortOrder.Descending);
        #endregion

        #endregion

    }
}
