
using System;

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;

using JetBrains.Annotations;
using Sql.IO;

namespace FtpServer.MsSqlFileSystem
{
    /// <summary>
    /// A <see cref="IUnixFileEntry"/> implementation for <see cref="MsSqlFileSystem"/> functionality.
    /// </summary>
    public class MsSqlFileEntry : IUnixFileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlFileEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The <see cref="MsSqlFileEntry"/> this entry belongs to.</param>
        /// <param name="info">The <see cref="SqlFileInfo"/> to extract the information from.</param>
        public MsSqlFileEntry([NotNull] MsSqlFileSystem fileSystem, [NotNull] SqlFileInfo info)
        {
            FileSystem = fileSystem;
            Info = info;
            LastWriteTime = info.Last_Write_Time;
            CreatedTime = info.Creation_Time;
            var accessMode = new GenericAccessMode(true, true, true);
            Permissions = new GenericUnixPermissions(accessMode, accessMode, accessMode);
        }

        /// <summary>
        /// Gets the underlying <see cref="SqlFileInfo"/>.
        /// </summary>
        public SqlFileInfo Info { get; }

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

        /// <inheritdoc/>
        public long Size => Info.Length;
    }
}
