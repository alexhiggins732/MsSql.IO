namespace Sql.IO
{
    /// <summary>
    /// Domain model for retrieving FileTable meta data from the Sql database.
    /// </summary>
    internal class SqlFileTableInfo
    {
        /// <summary>
        /// The native object_id for this file table in the Sql Server.
        /// </summary>
        public long object_id { get; set; }
        /// <summary>
        /// A flag indicating if the FileTable is enabled.
        /// </summary>
        public bool is_enabled { get; set; }
        /// <summary>
        /// The name of the directory where the contents of the file exists.
        /// </summary>
        public string directory_name { get; set; }
        /// <summary>
        /// Then name of the FileTable in the Sql Server database.
        /// </summary>
        public string table_name { get; set; }
    }
}


