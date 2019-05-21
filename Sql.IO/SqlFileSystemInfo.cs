using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.IO;

namespace Sql.IO
{
    /// <summary>
    /// Provides <see cref="System.IO.FileSystemInfo"/> style functionality and synchorizes underlying <see cref="SqlFileSystemEntry"/> metadata.
    /// </summary>
    public abstract class SqlFileSystemInfo
    {
        //TODO: Resolve connectionStringProvider using dependency injection or through a SqlContext
        /// <summary>
        /// A provider to supply connection string information to the server and database where the <see cref="SqlFileTable"/> for this <see cref="SqlFileSystemInfo'/> exists.
        /// </summary>
        protected IConnectionStringProvider connectionStringProvider;

        //TODO: Resolve filetable using dependency injection
        /// <summary>
        /// The <see cref="SqlFileTable"/> for this <see cref="SqlFileSystemInfo'/>.
        /// </summary>
        protected SqlFileTable fileTable;

        /// <summary>
        /// Initialize a new <see cref="SqlFileSystemInfo"/> based on the metadata in the specified <see cref="SqlFileSystemEntry"/>.
        /// </summary>
        /// <param name="entry">The metadata for the <see cref="SqlFileSystemInfo"/>.</param>
        /// <param name="connectionStringProvider">A connection string provider for accessing the underlying <see cref="SqlFileTable"/> for perform IO operations.</param>
        /// <param name="fileTable">The <see cref="SqlFileTable"/> for the <see cref="SqlDirectoryInfo"/>.</param>
        public SqlFileSystemInfo(SqlFileSystemEntry entry, IConnectionStringProvider connectionStringProvider, SqlFileTable fileTable)
        {
            this.connectionStringProvider = connectionStringProvider;
            this.fileTable = fileTable;
            UpdateMeta(entry);
        }

        /// <summary>
        /// Initializes a new <see cref="SqlFileSystemInfo"/> based on the specified path.
        /// </summary>
        /// <param name="fullPath">The full path to the directory.</param>
        public SqlFileSystemInfo(string fullPath)
        {
            this.FullName = fullPath;
            this.Name = Path.GetFileName(FullName);
            var info = SqlPathInfo.Parse(fullPath);
            //TODO: Cleanup fileTable initialization;
            var fileTable = SqlFileTable.GetSqlFileTables().FirstOrDefault(x => x.Directory_Name == info.FileTableDirectory);
            if (fileTable is null)
                return;
            this.fileTable = fileTable;

            //TODO: Cleanup connectionStringProvider initialization;
            this.connectionStringProvider = fileTable.connectionStringProvider;
            SqlFileSystemEntry dbEntry;
            if (info.IsFileTableDirectory)
            {
                dbEntry = new SqlFileSystemEntry()
                {
                    Name = fileTable.Directory_Name,
                    FullName = fullPath,
                    Is_Directory = true,
                    File_Type = null,
                };
            }
            else
            {
                dbEntry = fileTable.GetEntry(fullPath);
            }
            if (info.IsFileTableDirectory || dbEntry.Stream_Id != Guid.Empty)
            {
                UpdateMeta(dbEntry);
            }

        }

        /// <summary>
        /// Synchorized data with the specified <see cref="SqlFileSystemEntry"/>.
        /// </summary>
        /// <param name="dbEntry"></param>
        protected void UpdateMeta(SqlFileSystemEntry dbEntry)
        {
            Stream_Id = dbEntry.Stream_Id;
            Name = dbEntry.Name;
            FullName = dbEntry.FullName;
            Path_Locator = dbEntry.Path_Locator;
            Parent_Path_Locator = dbEntry.Parent_Path_Locator;
            File_Type = dbEntry.File_Type;
            Cached_File_Size = dbEntry.Cached_File_Size;
            Creation_Time = dbEntry.Creation_Time;
            Last_Write_Time = dbEntry.Last_Write_Time;
            Last_Access_Time = dbEntry.Last_Access_Time;
            Is_Archive = dbEntry.Is_Archive;
            Is_Directory = dbEntry.Is_Directory;
            Is_Hidden = dbEntry.Is_Hidden;
            Is_Offline = dbEntry.Is_Offline;
            Is_Readonly = dbEntry.Is_Readonly;
            Is_System = dbEntry.Is_System;
            Is_Temporary = dbEntry.Is_Temporary;
        }

        /// <summary>
        /// A <see cref="System.Data.SqlDbType.UniqueIdentifier"/> rowguidcol column for each FILESTREAM in the FILETABLE
        /// </summary>
        public virtual Guid Stream_Id { get; private set; }

        /// <summary>
        /// The name of the file or folder. The <see cref="MaxLengthAttribute"/> is 255.
        /// </summary>
        [MaxLength(255)]
        public virtual string Name { get; private set; }

        /// <summary>
        /// The full path and name of the file or foder. The <see cref="MaxLengthAttribute"/> is 260.
        /// </summary>
        [MaxLength(260)]
        public virtual string FullName { get; protected set; }

        /// <summary>
        /// The primary key of the table containing the heirarchyid for the file or folder.
        /// </summary>
        public virtual string Path_Locator { get; protected set; }

        /// <summary>
        /// A persisted computed column that represents the heirarchy id of the parent
        /// </summary>
        /// <remarks>For entries in the root of the FILETABLE (detected in the database using Path_Locator.GetLevel()=1) this value will be null
        /// When the entry is a child the database will use the GetLocator function./remarks>
        public virtual string Parent_Path_Locator { get; private set; }

        /// <summary>
        /// A persisted computed column that contains the extension of the file.
        /// Calculated using the database function getextension(name). 
        /// This value will be <see cref="null"/> when the file has no extension or <see cref="Is_Directory"/> is <see cref="True"/>.
        /// </summary>
        [MaxLength(255)]
        public virtual string File_Type { get; private set; }

        /// <summary>
        /// A cache of the <see cref="System.IO.FileInfo.Length" /> computed using datalength(file_stream).
        /// This value will be <see cref="null"/> when <see cref="Is_Directory"/> is <see cref="True"/>.
        /// There are rare edge cases in which this value could temporarily be out of sync.
        /// </summary>
        public virtual long? Cached_File_Size { get; private set; }

        /// <summary>
        /// Contains the <see cref="System.IO.FileSystemInfo.CreationTime"
        /// </summary>
        /// <remarks>Stored in the database with a precion of 34 and scale of 7</remarks>
        public virtual DateTimeOffset Creation_Time { get; private set; }

        /// <summary>
        /// Contains the <see cref="System.IO.FileSystemInfo.LastWriteTime"/>
        /// </summary>
        /// <remarks>Stored in the database with a precion of 34 and scale of 7</remarks>
        public virtual DateTimeOffset Last_Write_Time { get; private set; }

        /// <summary>
        /// Contains the <see cref="System.IO.FileSystemInfo.LastAccessTime"/>. 
        /// This will be <see cref="null"/> until the file is accessed.
        /// </summary>
        /// <remarks>Stored in the database with a precion of 34 and scale of 7</remarks>
        public virtual DateTimeOffset? Last_Access_Time { get; private set; }

        /// <summary>
        /// A flag to indicate if the entry is a directory. When the value is <see cref="False"/> the entry is a file.
        /// </summary>
        public virtual bool Is_Directory { get; protected set; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Offline"/> attribute is set.
        /// </summary>
        public virtual bool Is_Offline { get; private set; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Hidden"/> attribute is set.
        /// </summary>
        public virtual bool Is_Hidden { get; private set; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.ReadOnly"/> attribute is set.
        /// </summary>
        public virtual bool Is_Readonly { get; private set; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Archive"/> attribute is set.
        /// </summary>
        public virtual bool Is_Archive { get; private set; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.System"/> attribute is set.
        /// </summary>
        public virtual bool Is_System { get; private set; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Temporary"/> attribute is set.
        /// </summary>
        public virtual bool Is_Temporary { get; private set; }


        /// <summary>
        /// Returns true if this <see cref="SqlFileSystemInfo"/> exists by determining
        /// if there is a corresponding <see cref="SqlFileSystemEntry"/> in the underlying <see cref="SqlFileTable"/>.
        /// </summary>
        public bool Exists => fileTable.GetEntry(FullName).Stream_Id != Guid.Empty;

        /// <summary>
        /// Returns the parent <see cref="SqlDirectoryInfo"/> for this <see cref="SqlFileSystemInfo"/>.
        /// </summary>
        public virtual SqlDirectoryInfo Directory => new SqlDirectoryInfo(new FileInfo(FullName).Directory.FullName);

        /// <summary>
        /// Deletes this <see cref="SqlFileSystemInfo"/> from the underlying <see cref="SqlFileTable"/> which triggers a physical deletion from disk.
        /// If entry is a <see cref="SqlDirectoryInfo"/> and it is not empty an exception is thrown.
        /// To delete a directory and all of its contents use <see cref="Delete(bool)"/>
        /// </summary>
        public void Delete()
        {
            //TODO: Isolate data access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                //TODO: Cleanup embedded T-SQL
                var sql = $"Delete from [{this.fileTable.Table_Name}] where stream_Id=@stream_Id";
                conn.ExecuteScalar(sql, new { Stream_Id });
            }
        }

        /// <summary>
        /// Deletes this <see cref="SqlFileSystemInfo"/> from the underlying <see cref="SqlFileTable"/> which triggers a physical deletion from disk.
        /// If the directory is not empty an exception is thrown
        /// </summary>
        public void Delete(bool recursive)
        {
            if (recursive && Is_Directory)
            {
                var di = (SqlDirectoryInfo)this;
                var dirs = di.GetDirectories();
                foreach (var child in dirs)
                    child.Delete(recursive);
                var files = di.GetFiles();
                foreach (var file in files)
                    file.Delete();
            }

            //TODO: Isolate data access
            using (var conn = new SqlConnection(connectionStringProvider.ConnectionString))
            {
                //TODO: Cleanup embedded T-SQL
                var sql = $"Delete from [{this.fileTable.Table_Name}] where stream_Id={DbConstants.StreamIdParameterName}";
                conn.ExecuteScalar(sql, new { Stream_Id });
            }
        }


    }



}


