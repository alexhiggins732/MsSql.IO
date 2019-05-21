
using System;
using System.Linq;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;

using JetBrains.Annotations;
using Sql.IO;

namespace FtpServer.MsSqlFileSystem
{
    /// <summary>
    /// A <see cref="IUnixDirectoryEntry"/> implementation for <see cref="MsSqlFileSystem"/> functionality.
    /// </summary>
    public class MsSqlDirectoryEntry : IUnixDirectoryEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlDirectoryEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to.</param>
        /// <param name="dirInfo">The <see cref="SqlDirectoryInfo"/> to extract the information from.</param>
        public MsSqlDirectoryEntry([NotNull] MsSqlFileSystem fileSystem, [NotNull] SqlDirectoryInfo dirInfo)
        {
            FileSystem = fileSystem;
            Info = dirInfo;
            LastWriteTime = dirInfo.Last_Write_Time;
            CreatedTime = dirInfo.Creation_Time;
            var accessMode = new GenericAccessMode(true, true, true);
            Permissions = new GenericUnixPermissions(accessMode, accessMode, accessMode);
            IsRoot = dirInfo.Stream_Id == Guid.Empty;
        }

        /// <summary>
        /// Gets the underlying <see cref="SqlDirectoryInfo"/>.
        /// </summary>
        public SqlDirectoryInfo Info { get; }

        /// <inheritdoc/>
        public bool IsRoot { get; }

        /// <inheritdoc/>
        public bool IsDeletable => !IsRoot && (FileSystem.SupportsNonEmptyDirectoryDelete || !Info.EnumerateFileSystemInfos().Any());

        /// <inheritdoc/>
        public string Name => Info.Name;

        /// <inheritdoc/>
        public IUnixPermissions Permissions { get; }

        /// <inheritdoc/>
        public DateTimeOffset? LastWriteTime { get; }

        /// <inheritdoc/>
        public DateTimeOffset? CreatedTime { get; }

        /// <inheritdoc/>
        public long NumberOfLinks => 1;

        /// <inheritdoc/>
        public IUnixFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public string Owner => "owner";

        /// <inheritdoc/>
        public string Group => "group";
    }
}
