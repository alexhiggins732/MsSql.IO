using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IOTests
{
    public interface IUncInfo
    {
        string UncServerName { get; }
        string InstanceDirectory { get; }
        string DatabaseDirectory { get; }
        string FileTableDirectory { get; }
    }

    public class UncInfo : IUncInfo
    {
        public string UncServerName { get; }

        public string InstanceDirectory { get; }

        public string DatabaseDirectory { get; }

        public string FileTableDirectory { get; }

        public UncInfo()
        {
            UncServerName = ConfigurationManager.AppSettings[nameof(UncServerName)];
            InstanceDirectory = ConfigurationManager.AppSettings[nameof(InstanceDirectory)];
            DatabaseDirectory = ConfigurationManager.AppSettings[nameof(DatabaseDirectory)];
            FileTableDirectory = ConfigurationManager.AppSettings[nameof(FileTableDirectory)];
        }

        public UncInfo(string uncServerName, string InstanceDirectory, string databaseDirectory, string fileTableDirectory)
        {
            this.UncServerName = uncServerName;
            this.InstanceDirectory = InstanceDirectory;
            this.DatabaseDirectory = databaseDirectory;
            this.FileTableDirectory = fileTableDirectory;
        }

        public static UncInfo Default() => new UncInfo();
    }
}
