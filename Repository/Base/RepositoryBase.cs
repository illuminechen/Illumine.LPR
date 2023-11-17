using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Illumine.LPR.Repository
{
    public abstract class RepositoryBase
    {
        public string FileName { get; }

        public BaseExecutor Executor => ExecutorFactory.Create(this.FileName, this.RepoType);

        public List<PageBase> Pages { get; } = new List<PageBase>();

        public RepoType RepoType { get; }

        public RepositoryBase(string filename, RepoType repoType)
        {
            this.FileName = filename;
            this.RepoType = repoType;
        }

        public RepositoryBase(string fileName)
        {
            this.FileName = fileName;
            this.RepoType = RepoTypeHelper.GetRepoType(fileName);
        }

        public virtual void Create()
        {
            this.Executor.Create();
            this.Executor.CreatePages(this.Pages);
        }

        public virtual void Export(string targetPath, RepoType repoType)
        {
            BaseExecutor baseExecutor = ExecutorFactory.Create(targetPath, repoType);
            baseExecutor.Create();
            baseExecutor.CreatePages(this.Pages);
            var progress = Container.Get<ProgressDialog>();
            if (progress != null)
                progress.Dispatcher.Invoke(() => progress.Show());

            foreach (PageBase page in this.Pages)
            {
                var list = this.Executor.Read(page);
                progress.Dispatcher.Invoke(() =>
                {
                    progress.Maximum = list.Count;
                    progress.Caption = $"匯出{page.PageName}...";
                    progress.Value = 0;
                });

                foreach (object data in list)
                {
                    if (progress != null)
                    {
                        progress.Dispatcher.Invoke(() => {
                            progress.Value++;
                            progress.Caption = progress.Caption.Replace("...", "") + ".";
                        });
                    }
                    baseExecutor.Insert(page, data);
                }
            }
        }

        public virtual void Import(string sourcePage, RepoType repoType)
        {
            BaseExecutor baseExecutor = ExecutorFactory.Create(sourcePage, repoType);
            File.Delete(this.FileName + ".bak");
            File.Move(this.FileName, this.FileName + ".bak");
            this.Executor.Create();
            this.Executor.CreatePages(this.Pages);
            var progress = Container.Get<ProgressDialog>();
            if (progress != null)
                progress.Dispatcher.Invoke(() => progress.Show());
           
            foreach (PageBase page in this.Pages)
            {
                var list = baseExecutor.Read(page);
                progress.Dispatcher.Invoke(() =>
                {
                    progress.Maximum = list.Count;
                    progress.Caption = $"匯入{page.PageName}...";
                    progress.Value = 0;
                });
                foreach (object data in list)
                {
                    if (progress != null)
                    {
                        progress.Dispatcher.Invoke(() => {
                            progress.Value++;
                            progress.Caption = progress.Caption.Replace("...", "") + ".";
                        });
                    }
                    this.Executor.Insert(page, data);
                }
            }
        }

        public virtual void ReCreate()
        {
            var map = new Dictionary<string, List<object>>();
            foreach (var page in Pages)
            {
                map.Add(page.PageName, page.ReadList());
            }

            string filename = FileName + ".bak";

            // bakeup
            while (File.Exists(filename))
            {
                filename += ".bak";
            }

            File.Move(FileName, filename);

            Create();

            var progress = Container.Get<ProgressDialog>();
            if (progress != null)
                progress.Dispatcher.Invoke(() => progress.Show()) ;
            foreach (var page in Pages)
            {
                if (progress != null)
                {
                    progress.Dispatcher.Invoke(() =>
                    {
                        progress.Maximum = map[page.PageName].Count;
                        progress.Caption = $"更新{page.PageName}...";
                        progress.Value = 0;
                    });
                }
                foreach (var data in map[page.PageName])
                {
                    if (progress != null)
                    {
                        progress.Dispatcher.Invoke(() => { 
                            progress.Value++;
                            progress.Caption = progress.Caption.Replace("...", "") + ".";
                        });
                    }
                    page.InsertData(data);
                }
            }
        }
    }
}
