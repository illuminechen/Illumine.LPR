using System.IO;

namespace Illumine.LPR.Repository
{
    public class MsgRepository : RepositoryBase
    {
        public static MsgRepository Instance
        {
            get
            {
                MsgRepository instance = Container.Get<MsgRepository>();
                if (instance == null)
                {
                    instance = new MsgRepository(Container.Get<LPRSetting>().MsgPath);
                    Container.Put(instance);
                }
                return instance;
            }
        }

        public MsgPage Msg { get; }

        public MsgRepository(string filename)
          : base(filename)
        {
            this.Msg = new MsgPage(this);
            this.Pages.Add(Msg);
            if (File.Exists(filename))
                return;
            this.Create();
        }

        public MsgRepository(string filename, RepoType repoType)
          : base(filename, repoType)
        {
            this.Msg = new MsgPage(this);
            this.Pages.Add(Msg);
            if (File.Exists(filename))
                return;
            this.Create();
        }
    }
}
