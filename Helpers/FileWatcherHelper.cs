using System;
using System.IO;

namespace Illumine.LPR
{
    public class FileWatcherHelper
    {
        public static void RegisterWatcher(
          string FolderPath,
          string Filter,
          Action<object, FileSystemEventArgs> OnCreated)
        {
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.Path = FolderPath;
            fileSystemWatcher.Filter = Filter;
            fileSystemWatcher.Created += new FileSystemEventHandler(OnCreated.Invoke);
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}
