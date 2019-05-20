using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.IO;

namespace Sql.IO
{
    /// <summary>
    /// Provides <see cref="System.IO.DirectoryInfo"/> and <see cref="System.IO.FileSystemInfo"/> style functionality
    /// for a directory in the <see cref="SqlFileTable"/>.
    /// </summary>
    public class SqlDirectoryInfo : SqlFileSystemInfo
    {
        /// <summary>
        /// Initialize a new <see cref="SqlDirectoryInfo"/> based on the metadata in the specified <see cref="SqlFileSystemEntry"/>.
        /// </summary>
        /// <param name="entry">The metadata for the <see cref="SqlDirectoryInfo"/>.</param>
        /// <param name="connectionStringProvider">A connection string provider for accessing the underlying <see cref="SqlFileTable"/> for perform IO operations.</param>
        /// <param name="fileTable">The <see cref="SqlFileTable"/> for the <see cref="SqlDirectoryInfo"/>.</param>
        public SqlDirectoryInfo(SqlFileSystemEntry entry, IConnectionStringProvider connectionStringProvider, SqlFileTable fileTable)
           : base(entry, connectionStringProvider, fileTable)
        {
            if (entry.Is_Directory)
                if (!entry.Is_Directory)
                    throw new ArgumentException(Constants.EntryIsNotDirectory, nameof(entry));
        }

        /// <summary>
        /// Initializes a new <see cref="SqlDirectoryInfo"/> based on the specified path.
        /// </summary>
        /// <param name="fullPath">The full path to the directory.</param>
        public SqlDirectoryInfo(string fullPath) : base(fullPath) { }

        /// <summary>
        /// The name of the <see cref="SqlDirectoryInfo"/>.
        /// </summary>
        public override string Name => Path.GetFileName(FullName);

        /// <summary>
        /// Moves the directory and all of it's contents to the destination path
        /// </summary>
        /// <param name="destinationPath"></param>
        public void MoveTo(string destinationPath)
        {
            var destDi = new SqlDirectoryInfo(destinationPath);

            if (!destDi.Exists)
                destDi.Create();

            var subDirectories = GetDirectories();
            foreach (var dir in subDirectories)
            {
                dir.MoveTo(System.IO.Path.Combine(destDi.FullName, dir.Name));
            }

            var files = GetFiles();
            foreach (var file in files)
                file.MoveTo(System.IO.Path.Combine(destDi.FullName, file.Name));

            Delete();

        }

        /// <summary>
        /// File Type is not implemented for a <see cref="SqlDirectoryInfo"/>. Throws a <see cref="NotImplementedException"/>
        /// </summary>
        public override string File_Type => throw new NotImplementedException();

        /// <summary>
        /// Cached_File_Size is not implemented for a <see cref="SqlDirectoryInfo"/>. Throws a <see cref="NotImplementedException"/>
        /// </summary>
        public override long? Cached_File_Size => throw new NotImplementedException();

        /// <summary>
        /// A flag indicating the underlying <see cref="SqlFileSystemInfo"/> is a directory.
        /// </summary>
        public override bool Is_Directory
        {
            get => base.Is_Directory;
            protected set => base.Is_Directory = value ? value : throw new NotImplementedException(Constants.EntryIsNotDirectory);
        }


        ///// <summary>
        ///// Gets a list of sub directories that exist in the current directory.
        ///// </summary>
        ///// <returns></returns>
        //public List<SqlDirectoryInfo> GetDirectories2()
        //{
        //    var entries = SqlContext.Current.GetChildDirectories(fileTable.Table_Name, new { Path_Locator });
        //    return entries.Select(x => new SqlDirectoryInfo(x, this.fileTable));
        //}
        /// <summary>
        /// Gets a list of sub directories that exist in the current directory.
        /// </summary>
        /// <returns></returns>
        public List<SqlDirectoryInfo> GetDirectories()
        {

            //TODO: Cleanup embedded T-SQL
            var sql = $@"
SELECT     
 {DbConstants.FileTableSelectList}
FROM 
	[{fileTable.Table_Name}]
WHERE [is_directory] =1 and [parent_path_locator].ToString()={DbConstants.PathLocatorParameterName}
";
            //TODO: seperate data access logic
            var result = new List<SqlDirectoryInfo>();
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();

                result = conn.Query<SqlFileSystemEntry>(sql, new { this.Path_Locator })
                    .Select(x => new SqlDirectoryInfo(x, connectionStringProvider, this.fileTable))
                    .ToList();
            }
            return result;
        }

        /// <summary>
        /// Gets a list of files that exist in the current directory.
        /// </summary>
        /// <returns></returns>
        public List<SqlFileInfo> GetFiles()
        {
            //TODO: Cleanup embedded T-SQL
            var sql = $@"
select     
 {DbConstants.FileTableSelectList}
from 
	[{fileTable.Table_Name}]
where is_directory=0 and [parent_path_locator].ToString()={DbConstants.PathLocatorParameterName}
";
            //TODO: seperate data access logic
            var result = new List<SqlFileInfo>();
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();
                result = conn.Query<SqlFileSystemEntry>(sql, new { this.Path_Locator })
                    .Select(x => new SqlFileInfo(x, connectionStringProvider, this.fileTable))
                    .ToList();
            }
            return result;
        }

        /// <summary>
        /// Creates a new directory.
        /// </summary>
        public void Create()
        {
            var parentDirectory = this.Directory;
            string path_locator = "";
            string sql = "";

            //TODO: need to recursively create parent directories.
            if (parentDirectory.Path_Locator is null)
            {
                //TODO: Cleanup embedded T-SQL
                //If this is a root directory in the file table SQL will generate a path_locator
                sql = $@"
insert into [{fileTable.Table_Name}]
([name], [is_directory], [is_archive])
values
({DbConstants.NameParameterName}, 1, 0)  
";
            }
            else
            {
                //Otherwise, we need to specify a path_locator under the parent's path_locator
                //  to prevent SQL from creating the directory as root directory in the file table.
                var locator = SqlLocatorId.NewId();

                //TODO :Provide a utility for combining locator strings.
                path_locator = $"{parentDirectory.Path_Locator}{locator}/";

                //TODO: Cleanup embedded T-SQL
                sql = sql = $@"
insert into [{base.fileTable.Table_Name}]
([name], [is_directory], [is_archive], [path_locator])
values
({DbConstants.NameParameterName}, 1, 0,{DbConstants.PathLocatorParameterName})  
";
            }

            //TODO: Isolate database access. 
            var result = new List<SqlDirectoryInfo>();
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();

                result = conn.Query<SqlFileSystemEntry>(sql, new { this.Name, path_locator })
                    .Select(x => new SqlDirectoryInfo(x, connectionStringProvider, this.fileTable))
                    .ToList();
            }

            UpdateMeta(fileTable.GetEntry(FullName));
        }

    }



}


