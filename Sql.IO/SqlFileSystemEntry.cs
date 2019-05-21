using System;
using System.ComponentModel.DataAnnotations;

namespace Sql.IO
{
    /// <summary>
    /// Domain object model of SQL FileTable records that provide meta data for <see cref="SqlFileSystemInfo"/> implementations
    /// the <see cref="SqlFileInfo"/> and <see cref="SqlDirectoryInfo"/>.
    /// </summary>
    public struct SqlFileSystemEntry
    {
        /// <summary>
        /// A <see cref="System.Data.SqlDbType.UniqueIdentifier"/> rowguidcol column for each FILESTREAM in the FILETABLE
        /// </summary>
        public Guid Stream_Id { get; }

        /// <summary>
        /// The name of the file or folder. The <see cref="MaxLengthAttribute"/> is 255.
        /// </summary>
        [MaxLength(255)]
        public string Name { get; internal set; }

        /// <summary>
        /// The full path and name of the file or foder. The <see cref="MaxLengthAttribute"/> is 260.
        /// </summary>
        [MaxLength(260)]
        public string FullName { get; set; }

        /// <summary>
        /// The primary key of the table containing the heirarchyid for the file or folder.
        /// </summary>
        public string Path_Locator { get; }

        /// <summary>
        /// A persisted computed column that represents the heirarchy id of the parent
        /// </summary>
        /// <remarks>For entries in the root of the FILETABLE (detected in the database using Path_Locator.GetLevel()=1) this value will be null
        /// When the entry is a child the database will use the GetLocator function./remarks>
        public string Parent_Path_Locator { get; }

        /// <summary>
        /// A persisted computed column that contains the extension of the file.
        /// Calculated using the database function getextension(name). 
        /// This value will be <see cref="null"/> when the file has no extension or <see cref="Is_Directory"/> is <see cref="True"/>.
        /// </summary>
        [MaxLength(255)]
        public string File_Type { get; internal set; }

        /// <summary>
        /// A cache of the <see cref="System.IO.FileInfo.Length" /> computed using datalength(file_stream).
        /// This value will be <see cref="null"/> when <see cref="Is_Directory"/> is <see cref="True"/>.
        /// There are rare edge cases in which this value could temporarily be out of sync.
        /// </summary>
        public long? Cached_File_Size { get; }

        /// <summary>
        /// Contains the <see cref="System.IO.FileSystemInfo.CreationTime"
        /// </summary>
        /// <remarks>Stored in the database with a precion of 34 and scale of 7</remarks>
        public DateTimeOffset Creation_Time { get; }

        /// <summary>
        /// Contains the <see cref="System.IO.FileSystemInfo.LastWriteTime"/>
        /// </summary>
        /// <remarks>Stored in the database with a precion of 34 and scale of 7</remarks>
        public DateTimeOffset Last_Write_Time { get; }

        /// <summary>
        /// Contains the <see cref="System.IO.FileSystemInfo.LastAccessTime"/>. 
        /// This will be <see cref="null"/> until the file is accessed.
        /// </summary>
        /// <remarks>Stored in the database with a precion of 34 and scale of 7</remarks>
        public DateTimeOffset? Last_Access_Time { get; }

        /// <summary>
        /// A flag to indicate if the entry is a directory. When the value is <see cref="False"/> the entry is a file.
        /// </summary>
        public bool Is_Directory { get; internal set; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Offline"/> attribute is set.
        /// </summary>
        public bool Is_Offline { get; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Hidden"/> attribute is set.
        /// </summary>
        public bool Is_Hidden { get; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.ReadOnly"/> attribute is set.
        /// </summary>
        public bool Is_Readonly { get; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Archive"/> attribute is set.
        /// </summary>
        public bool Is_Archive { get; }

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.System"/> attribute is set.
        /// </summary>
        public bool Is_System { get;}

        /// <summary>
        /// A flag representing if the <see cref="System.IO.FileAttributes.Temporary"/> attribute is set.
        /// </summary>
        public bool Is_Temporary { get; }
    }



}


