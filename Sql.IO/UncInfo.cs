using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IO
{
    /// <summary>
    /// Provides path of UNC share to allow files to be queried by their SQL generated Path Locator.
    /// </summary>
    public class UncInfo : IUncInfo
    {

        /// <summary>
        /// Returns the default <see cref="UncInfo"/> instance populated with settings from the application's configuration file.
        /// </summary>
        /// <returns>A <see cref="UncInfo"/> with settings read from the application's config file.</returns>
        public static UncInfo Default() => new UncInfo();

        /// <summary>
        /// The SQL Server or Virtual Network Name.
        /// </summary>
        public string UncServerName { get; }

        /// <summary>
        /// The name of the instance share directory
        /// </summary>
        public string InstanceDirectory { get; }

        /// <summary>
        /// The name of the database share directory.
        /// </summary>
        public string DatabaseDirectory { get; }

        /// <summary>
        /// The name of the FILETABLE directory.
        /// </summary>
        public string FileTableDirectory { get; }

        /// <summary>
        /// Initialize a <see cref="UncInfo"/> instance populated with settings from the application's configuration file.
        /// </summary>
        public UncInfo()
        {
            UncServerName = ConfigurationManager.AppSettings[nameof(UncServerName)];
            InstanceDirectory = ConfigurationManager.AppSettings[nameof(InstanceDirectory)];
            DatabaseDirectory = ConfigurationManager.AppSettings[nameof(DatabaseDirectory)];
            FileTableDirectory = ConfigurationManager.AppSettings[nameof(FileTableDirectory)];
        }

        /// <summary>
        /// Initialize a <see cref="UncInfo"/> instance populated the specified parameters.
        /// </summary>
        public UncInfo(string uncServerName, string instanceDirectory, string databaseDirectory, string fileTableDirectory)
        {
            this.UncServerName = uncServerName;
            this.InstanceDirectory = instanceDirectory;
            this.DatabaseDirectory = databaseDirectory;
            this.FileTableDirectory = fileTableDirectory;
        }

        /// <summary>
        /// Returns a the UNC path used as a path locator by MS SQL Server for resolving file entries.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $@"\\{UncServerName}\{InstanceDirectory}\{DatabaseDirectory}\{FileTableDirectory}";
     
    }
}
