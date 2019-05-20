using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.IO;

namespace Sql.IO
{

    /// <summary>
    /// A class providing <see cref="System.IO"/> style functionality to directories and files saved in a Sql File Table.
    /// </summary>
    public class SqlFileTable
    {
        /// <summary>
        /// A provider to supply connection string information to the server and database where the <see cref="SqlFileTable"/> exists.
        /// </summary>
        public IConnectionStringProvider connectionStringProvider { get; protected set; }
        /// <summary>
        /// The native object_id for the <see cref="SqlFileTable"/> in the Sql Server.
        /// </summary>
        public long Object_Id { get; }
        /// <summary>
        /// A flag indicating if the <see cref="SqlFileTable"/> is enabled.
        /// </summary>
        public bool Is_Enabled { get; }
        /// <summary>
        /// The name of the directory where the contents of the <see cref="SqlFileTable"/> file exists on disk.
        /// </summary>
        public string Directory_Name { get; }
        /// <summary>
        /// Then Sql table name of the <see cref="SqlFileTable"/> in the Sql Server database.
        /// </summary>
        public string Table_Name { get; }

        /// <summary>
        /// Initializes a new <see cref="SqlFileTable"/>s to provide services for accessing <see cref="SqlFileSystemInfo"/> meta data defined in the File Table.
        /// </summary>
        /// <param name="connectionStringProvider"></param>
        /// <param name="objectId"></param>
        /// <param name="enabled"></param>
        /// <param name="directoryName"></param>
        /// <param name="tableName"></param>
        public SqlFileTable(IConnectionStringProvider connectionStringProvider,
            long objectId, bool enabled, string directoryName, string tableName)
        {
            this.connectionStringProvider = connectionStringProvider;
            this.Object_Id = objectId;
            this.Is_Enabled = enabled;
            this.Directory_Name = directoryName;
            this.Table_Name = tableName;
        }

        //TODO: Implement DI resolution for IConnectionStringProvider 
        /// <summary>
        /// Returns a <see cref="SqlFileTable"/> from the database based on its <see cref="Directory_Name"/>.
        /// </summary>
        /// <param name="connectionStringProvider"></param>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public static SqlFileTable GetSqlFileTable(IConnectionStringProvider connectionStringProvider, string directoryName)
        {
            SqlFileTable result = null;

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();
                //TODO: Cleanup embedded T-SQL
                var info = conn.QueryFirst<SqlFileTableInfo>($"select top 1 ft.object_id, ft.is_enabled, ft.directory_name, t.name as table_name from sys.filetables ft join sys.tables t on ft.object_id=t.object_id where is_filetable = 1 and ft.directory_name={DbConstants.DirectoryNameParameterName}", new { directoryName });
                result = new SqlFileTable(connectionStringProvider, info.object_id, info.is_enabled, info.directory_name, info.table_name);
            }
            return result;
        }

        //TODO: Resolve IConnectionStringProvider via DI
        /// <summary>
        /// Returns a list of all <see cref="SqlFileTable"/>s defined in the specified database.
        /// </summary>
        /// <param name="connectionStringProvider"></param>
        /// <returns></returns>
        public static List<SqlFileTable> GetSqlFileTables(IConnectionStringProvider connectionStringProvider = null)
        {
            if (connectionStringProvider == null)
                connectionStringProvider = SqlContext.Current.ConnectionStringProvider;

            //TODO: Isolate database access
            var result = new List<SqlFileTable>();
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();

                var tableInfos = conn.Query<SqlFileTableInfo>("select ft.object_id, ft.is_enabled, ft.directory_name, t.name as table_name from sys.filetables ft join sys.tables t on ft.object_id=t.object_id where is_filetable = 1");
                foreach (var info in tableInfos)
                {
                    result.Add(new SqlFileTable(connectionStringProvider, info.object_id, info.is_enabled, info.directory_name, info.table_name));
                }
            }
            return result;
        }


        /// <summary>
        /// Returns metadata for every <see cref="SqlFileSystemEntry"/> row in the <see cref="SqlFileTable"/>. 
        /// </summary>
        /// <returns></returns>
        public List<SqlFileSystemEntry> GetAllEntries()
        {
            var result = new List<SqlFileSystemEntry>();

            //TODO: Cleanup embedded T-SQL
            var sql = $@"
SELECT 
 {DbConstants.FileTableSelectList}
	, FileTableRootPath() + file_stream.GetFileNamespacePath() as [FullName]
FROM 
    [{Table_Name}]
";

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();
                result = conn.Query<SqlFileSystemEntry>(sql).ToList();
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all <see cref="SqlDirectoryInfo"/> that exist in the root of the <see cref="SqlFileTable"/>.
        /// This method provides the functionality similar to <see cref="SqlDirectoryInfo.GetDirectories"/>.
        /// </summary>
        /// <returns></returns>
        public List<SqlDirectoryInfo> GetDirectories()
        {

            //TODO: Cleanup embedded T-SQL
            var sql = $@"
select     
 {DbConstants.FileTableSelectList}
from 
   [{Table_Name}]
where is_directory=1 and [parent_path_locator] is null
";
            var result = new List<SqlDirectoryInfo>();

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();

                result = conn.Query<SqlFileSystemEntry>(sql)
                    .Select(x => new SqlDirectoryInfo(x, connectionStringProvider, this))
                    .ToList();
            }
            return result;
        }

        /// <summary>
        ///  Returns a list of all <see cref="SqlFileInfo"/> that exist in the root of the <see cref="SqlFileTable"/>. 
        /// This method provides the functionality similar to <see cref="SqlDirectoryInfo.GetFiles"/>.
        /// </summary>
        /// <returns></returns>
        public List<SqlFileInfo> GetFiles()
        {

            //TODO: Cleanup embedded T-SQL
            var sql = $@"
select     
 {DbConstants.FileTableSelectList}
from 
   [{Table_Name}]
where is_directory=0 and [parent_path_locator] is null
";
            var result = new List<SqlFileInfo>();

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();

                result = conn.Query<SqlFileSystemEntry>(sql)
                    .Select(x => new SqlFileInfo(x, connectionStringProvider, this))
                    .ToList();
            }
            return result;
        }

        /// <summary>
        /// Retrieves a <see cref="SqlFileInfo"/> from the <see cref="SqlFileTable"/> based on its <paramref name="stream_Id"/>.
        /// If the record does not exist in the database a <see cref="FileNotFoundException"/> is thrown.
        /// </summary>
        /// <param name="stream_Id"></param>
        /// <returns></returns>
        public SqlFileInfo GetFile(Guid stream_Id)
        {
            //TODO: Cleanup embedded T-SQL
            var sql = $@"
select     
 {DbConstants.FileTableSelectList}
from 
   [{Table_Name}]
where is_directory=0 and stream_id={DbConstants.StreamIdParameterName};
";

            SqlFileSystemEntry entry;

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();
                entry = conn.QueryFirstOrDefault<SqlFileSystemEntry>(sql, new { stream_Id });

            }
            if (entry.Stream_Id == Guid.Empty)
            {
                throw new FileNotFoundException(Constants.FileDoesNotExists);
            }
            return new SqlFileInfo(entry, connectionStringProvider, this);
        }


        /// <summary>
        /// Retrieves a <see cref="SqlDirectoryInfo"/> from the <see cref="SqlFileTable"/> based on its <paramref name="stream_Id"/>.
        /// If the record does not exist in the database a <see cref="DirectoryNotFoundException"/> is thrown.
        /// </summary>
        /// <param name="stream_Id"></param>
        /// <returns></returns>
        public SqlDirectoryInfo GetDirectory(Guid stream_Id)
        {
            //TODO: Cleanup embedded T-SQL
            var sql = $@"
select     
 {DbConstants.FileTableSelectList}
from 
   [{Table_Name}]
where is_directory=1 and stream_id={DbConstants.StreamIdParameterName};
";

            SqlFileSystemEntry entry;

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();
                entry = conn.QueryFirstOrDefault<SqlFileSystemEntry>(sql);
            }

            if (entry.Stream_Id == Guid.Empty)
            {
                throw new DirectoryNotFoundException(Constants.DirectoryDoesNotExist);
            }
            return new SqlDirectoryInfo(entry, connectionStringProvider, this);
        }

        /// <summary>
        /// Internal method to retriee the metadata for a <see cref="SqlFileSystemEntry"/> based on it's path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal SqlFileSystemEntry GetEntry(string path)
        {
            var info = SqlPathInfo.Parse(path);
            var relativePath = info.RelativePath;

            //TODO: Cleanup embedded T-SQL
            var sql = $@"

SELECT 
 {DbConstants.FileTableSelectList}
FROM 
   [{Table_Name}]
where file_stream.GetFileNamespacePath() = {DbConstants.RelativePathParameterName}
";

            SqlFileSystemEntry entry;

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();
                entry = conn.QueryFirstOrDefault<SqlFileSystemEntry>(sql, new { relativePath });

            }
            return entry;
        }
    }



}


