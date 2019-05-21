namespace Sql.IO
{
    /// <summary>
    /// Service for providing MS SQL UNC share path.
    /// </summary>
    public interface IUncInfo
    {
        /// <summary>
        /// The SQL Server or Virtual Network Name.
        /// </summary>
        string UncServerName { get; }

        /// <summary>
        /// The name of the instance share directory
        /// </summary>
        string InstanceDirectory { get; }

        /// <summary>
        /// The name of the database share directory.
        /// </summary>
        string DatabaseDirectory { get; }

        /// <summary>
        /// The name of the FILETABLE directory.
        /// </summary>
        string FileTableDirectory { get; }
    }
}
