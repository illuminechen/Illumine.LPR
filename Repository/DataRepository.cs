using System.IO;

namespace Illumine.LPR.Repository
{
    public class DataRepository : RepositoryBase
    {
        public static DataRepository Instance
        {
            get
            {
                DataRepository instance = Container.Get<DataRepository>();
                if (instance == null)
                {
                    instance = new DataRepository(Container.Get<LPRSetting>().DataPath);
                    Container.Put<DataRepository>(instance);
                }
                return instance;
            }
        }

        public VipPage Vip { get; }
        public GroupPage Group { get; }

        public DataRepository(string filename)
          : base(filename)
        {
            this.Vip = new VipPage((RepositoryBase)this);
            this.Group = new GroupPage((RepositoryBase)this);
            this.Pages.Add((PageBase)this.Vip);
            this.Pages.Add((PageBase)this.Group);
            if (File.Exists(filename))
                return;
            this.Create();
        }

        public DataRepository(string filename, RepoType repoType)
          : base(filename, repoType)
        {
            this.Vip = new VipPage((RepositoryBase)this);
            this.Group = new GroupPage((RepositoryBase)this);
            this.Pages.Add((PageBase)this.Vip);
            this.Pages.Add((PageBase)this.Group);
            if (File.Exists(filename))
                return;
            this.Create();
        }

    }
}
