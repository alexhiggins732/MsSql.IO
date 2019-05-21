using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.IO;

namespace Sql.IO
{
    /// <summary>
    /// Provides <see cref="System.IO.FileInfo"/> and <see cref="System.IO.FileSystemInfo"/> style functionality
    /// for a file in the <see cref="SqlFileTable"/>.
    /// </summary>
    public class SqlFileInfo : SqlFileSystemInfo
    {
        /// <summary>
        /// Initialize a new <see cref="SqlFileInfo"/> based on the metadata in the specified <see cref="SqlFileSystemEntry"/>.
        /// </summary>
        /// <param name="entry">The metadata for the <see cref="SqlFileInfo"/>.</param>
        /// <param name="connectionStringProvider">A connection string provider for accessing the underlying <see cref="SqlFileTable"/> for perform IO operations.</param>
        /// <param name="fileTable">The <see cref="SqlFileTable"/> for the <see cref="SqlFileInfo"/>.</param>
        public SqlFileInfo(SqlFileSystemEntry entry, IConnectionStringProvider connectionStringProvider, SqlFileTable fileTable)
           : base(entry, connectionStringProvider, fileTable)
        {
            if (entry.Is_Directory)
                throw new ArgumentException(Constants.EntryIsNotFile, nameof(entry));

        }
        /// <summary>
        /// Initializes a <see cref="SqlFileSystemInfo"/> based on the specified path.
        /// </summary>
        /// <param name="path">The full path to the file.</param>
        public SqlFileInfo(string path) : base(path)
        {
            if (Is_Directory)
                throw new ArgumentException(Constants.EntryIsNotFile);
        }

        /// <summary>
        /// Returns <see cref="SqlFileStream"/> for the actual file content, This value is null for directories or empty for files with no content.
        /// </summary>
        /// <returns>The underlying <see cref="Stream"/> for this <see cref="SqlFileInfo"/>.</returns>
        public Stream File_Stream() => new SqlFileStream(this.Stream_Id, this.fileTable);


        /// <summary>
        /// Opens and existing file overwrites its contents and returns a new stream for performing IO operations on the underlying file content.
        /// </summary>
        /// <returns>The underlying <see cref="Stream"/> for this <see cref="SqlFileInfo"/> after clearing its content.</returns>
        public Stream OpenNew() => new SqlFileStream(this.Stream_Id, this.fileTable, FileMode.CreateNew);

        /// <summary>
        /// Returns a <see cref="Stream"/> of the actual file content for an existing file for reading.
        /// </summary>
        /// <returns>The underlying <see cref="Stream"/> for this <see cref="SqlFileInfo"/>.</returns>
        public Stream OpenRead() => File_Stream();


        /// <summary>
        /// Returns a <see cref="Stream"/> of the actual file content for an existing file for writing
        /// </summary>
        /// <returns>The underlying <see cref="Stream"/> for this <see cref="SqlFileInfo"/>.</returns>
        public Stream OpenWrite() => File_Stream();

        /// <summary>
        /// Creates a new file and returns a references to a stream for performing IO operations on the underlying file content.
        /// </summary>
        /// <returns>The underlying <see cref="Stream"/> for this <see cref="SqlFileInfo"/>.</returns>
        public Stream Create()
        {
            if (!this.Exists)
            {
                var sql = "";
                var path_locator = "";
                var parentDirectory = this.Directory;
                if (!parentDirectory.Exists)
                    throw new System.IO.DirectoryNotFoundException(Constants.ParentDirectoryDoesNotExist);

                if (parentDirectory.Path_Locator is null)
                {
                    //TODO: Cleanup embedded T-SQL
                    //If this is a root directory in the file table SQL will generate a path_locator
                    sql = $@"
insert into [{base.fileTable.Table_Name}]
([name], [file_stream],[is_directory], [is_archive])
values
(@Name, 0x, 0, 0)  
";
                }
                else
                {
                    //Otherwise, we need to specify a path_locator under the parent's path_locator
                    //  to prevent SQL from creating the directory as root directory in the file table.
                    var locator = SqlLocatorId.NewId();

                    //TODO: provide a utility for combining locators
                    path_locator = $"{parentDirectory.Path_Locator}{locator}/";

                    //TODO: Cleanup embedded T-SQL
                    sql = sql = $@"
insert into [{base.fileTable.Table_Name}]
([name], [file_stream], [is_directory], [is_archive], [path_locator])
values
(@name, 0x, 0, 0,@path_locator)  
";
                }

                //TODO: Isolate database access
                var result = new List<SqlFileInfo>();
                using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
                {
                    conn.Open();

                    result = conn.Query<SqlFileSystemEntry>(sql, new { this.Name, path_locator })
                        .Select(x => new SqlFileInfo(x, connectionStringProvider, this.fileTable))
                        .ToList();
                }

                base.UpdateMeta(fileTable.GetEntry(FullName));
            }
            return OpenNew();
        }

        /// <summary>
        /// Flag to indicate the underlying <see cref="SqlFileSystemInfo"/> is a file and not a directory.
        /// </summary>
        /// <returns>Returns <see cref="false"/> or throws a <see cref="NotSupportedException"/> if the base <see cref="SqlFileSystemInfo"/> is <see cref="SqlDirectoryInfo"/>.</returns>
        public override bool Is_Directory
        {
            get => base.Is_Directory;
            protected set
            {
                if (value)
                    throw new NotSupportedException(Constants.EntryIsNotFile +  " "+  FullName);
                base.Is_Directory = value;
            }
        }


        /// <summary>
        /// Moves the file to the specified destination path
        /// </summary>
        /// <param name="destinationPath"></param>
        public void MoveTo(string destinationPath)
        {
            var fi = new SqlFileInfo(destinationPath);
            var parent_locator = fi.Directory?.Path_Locator is null ? "/" : fi.Directory?.Path_Locator;
            var new_path_locator = $"{parent_locator}{this.Stream_Id.ToSqlLocator()}/";

            //TODO: Cleanup embedded T-SQL
            var sql = $@"
update [{fileTable.Table_Name}]
    set path_locator=@new_path_locator
    where stream_id=@stream_Id
";

            //TODO: Isolate database access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                conn.Open();
                conn.Execute(sql, new { Stream_Id, new_path_locator });
            }
        }

        /// <summary>
        /// Returns the cached file size of the underlying file content.
        /// </summary>
        /// <returns>The cached file size of the underlying file content.</returns>
        public long Length => Cached_File_Size.GetValueOrDefault();
    }



}


