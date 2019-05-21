using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.FileSystem;
using Sql.IO;

namespace FtpServer.MsSqlFileSystem
{
    /// <summary>
    /// A <see cref="IUnixFileSystem"/> implementation that uses the
    /// MS SQL FileTables and FileStreams to provide file system functionality.
    /// </summary>
    public class MsSqlFileSystem : IUnixFileSystem
    {
        /// <summary>
        /// The default buffer size for copying from one stream to another.
        /// </summary>
        public static readonly int DefaultStreamBufferSize = 4096;

        private readonly int _streamBufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlFileSystem"/> class.
        /// </summary>
        /// <param name="rootPath">The path to use as root.</param>
        /// <param name="allowNonEmptyDirectoryDelete">Defines whether the deletion of non-empty directories is allowed.</param>
        public MsSqlFileSystem(string rootPath, bool allowNonEmptyDirectoryDelete)
            : this(rootPath, allowNonEmptyDirectoryDelete, DefaultStreamBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlFileSystem"/> class.
        /// </summary>
        /// <param name="rootPath">The path to use as root.</param>
        /// <param name="allowNonEmptyDirectoryDelete">Defines whether the deletion of non-empty directories is allowed.</param>
        /// <param name="streamBufferSize">Buffer size to be used in async IO methods.</param>
        public MsSqlFileSystem(string rootPath, bool allowNonEmptyDirectoryDelete, int streamBufferSize)
        {
            FileSystemEntryComparer = StringComparer.OrdinalIgnoreCase;
            Root = new MsSqlDirectoryEntry(this, SqlDirectory.CreateDirectory(rootPath));
            SupportsNonEmptyDirectoryDelete = allowNonEmptyDirectoryDelete;
            _streamBufferSize = streamBufferSize;
        }

        /// <inheritdoc/>
        public bool SupportsNonEmptyDirectoryDelete { get; }

        /// <inheritdoc/>
        public StringComparer FileSystemEntryComparer { get; }

        /// <inheritdoc/>
        public IUnixDirectoryEntry Root { get; }

        /// <inheritdoc/>
        public bool SupportsAppend => true;

        /// <inheritdoc/>
        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var result = new List<IUnixFileSystemEntry>();
            var searchDirInfo = ((MsSqlDirectoryEntry)directoryEntry).Info;
            foreach (var info in searchDirInfo.EnumerateFileSystemInfos())
            {
                if (info is SqlDirectoryInfo dirInfo)
                {
                    result.Add(new MsSqlDirectoryEntry(this, dirInfo));
                }
                else
                {
                    if (info is SqlFileInfo fileInfo)
                    {
                        result.Add(new MsSqlFileEntry(this, fileInfo));
                    }
                }
            }
            return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(result);
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var searchDirInfo = ((MsSqlDirectoryEntry)directoryEntry).Info;
            var fullPath = Path.Combine(searchDirInfo.FullName, name);
            IUnixFileSystemEntry result;
            var entry = SqlPath.GetFileSystemInfo(fullPath);
            if (entry != null)
            {
                if (entry is SqlFileInfo fileInfo)
                {
                    result = new MsSqlFileEntry(this, fileInfo);
                }
                else if (entry is SqlDirectoryInfo directoryInfo)
                {
                    result = new MsSqlDirectoryEntry(this, directoryInfo);
                }
                else
                {
                    result = null;
                }
            }
            else
            {
                result = null;
            }
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            var targetEntry = (MsSqlDirectoryEntry)target;
            var targetName = Path.Combine(targetEntry.Info.FullName, fileName);

            if (source is MsSqlFileEntry sourceFileEntry)
            {
                sourceFileEntry.Info.MoveTo(targetName);
                return Task.FromResult<IUnixFileSystemEntry>(new MsSqlFileEntry(this, new SqlFileInfo(targetName)));
            }

            var sourceDirEntry = (MsSqlDirectoryEntry)source;
            sourceDirEntry.Info.MoveTo(targetName);
            return Task.FromResult<IUnixFileSystemEntry>(new MsSqlDirectoryEntry(this, new SqlDirectoryInfo(targetName)));
        }

        /// <inheritdoc/>
        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            if (entry is MsSqlDirectoryEntry dirEntry)
            {
                dirEntry.Info.Delete(SupportsNonEmptyDirectoryDelete);
            }
            else
            {
                var fileEntry = (MsSqlFileEntry)entry;
                fileEntry.Info.Delete();
            }

            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            var targetEntry = (MsSqlDirectoryEntry)targetDirectory;
            var newDirInfo = targetEntry.Info.CreateSubdirectory(directoryName);
            return Task.FromResult<IUnixDirectoryEntry>(new MsSqlDirectoryEntry(this, newDirInfo));
        }

        /// <inheritdoc/>
        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            var fileInfo = ((MsSqlFileEntry)fileEntry).Info;
            var input = fileInfo.OpenRead();
            if (startPosition != 0)
            {
                input.Seek(startPosition, SeekOrigin.Begin);
            }

            return Task.FromResult<Stream>(input);
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            var fileInfo = ((MsSqlFileEntry)fileEntry).Info;
            using (var output = fileInfo.OpenWrite())
            {
                if (startPosition == null)
                {
                    startPosition = fileInfo.Length;
                }

                output.Seek(startPosition.Value, SeekOrigin.Begin);
                await data.CopyToAsync(output, _streamBufferSize, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            var targetEntry = (MsSqlDirectoryEntry)targetDirectory;
            var fileInfo = new SqlFileInfo(Path.Combine(targetEntry.Info.FullName, fileName));
            using (var output = fileInfo.Create())
            {
                await data.CopyToAsync(output, _streamBufferSize, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var fileInfo = ((MsSqlFileEntry)fileEntry).Info;
            using (var output = fileInfo.OpenWrite())
            {
                await data.CopyToAsync(output, _streamBufferSize, cancellationToken).ConfigureAwait(false);
                output.SetLength(output.Position);
            }

            return null;
        }

        /// <summary>
        /// Sets the modify/access/create timestamp of a file system item.
        /// </summary>
        /// <param name="entry">The <see cref="IUnixFileSystemEntry"/> to change the timestamp for.</param>
        /// <param name="modify">The modification timestamp.</param>
        /// <param name="access">The access timestamp.</param>
        /// <param name="create">The creation timestamp.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The modified <see cref="IUnixFileSystemEntry"/>.</returns>
        public Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            SqlFileSystemInfo item;
            if (entry is MsSqlDirectoryEntry dirEntry)
            {
                item = dirEntry.Info;
            }
            else if (entry is MsSqlFileEntry fileEntry)
            {
                item = fileEntry.Info;
                dirEntry = null;
            }
            else
            {
                throw new ArgumentException("Argument must be of type MsSqlDirectoryEntry or MsSqlFileEntry", nameof(entry));
            }

            if (access != null)
            {
                //TODO: Implement
                //item.Last_Access_Time = access.Value.UtcDateTime;
            }

            if (modify != null)
            {
                //TODO: Implement
                //item.Last_Write_Time = modify.Value.UtcDateTime;
            }

            if (create != null)
            {
                //TODO: Implement
                //item.Creation_Time = create.Value.UtcDateTime;
            }

            if (dirEntry != null)
            {
                return Task.FromResult<IUnixFileSystemEntry>(new MsSqlDirectoryEntry(this, (SqlDirectoryInfo)item));
            }

            return Task.FromResult<IUnixFileSystemEntry>(new MsSqlFileEntry(this, (SqlFileInfo)item));
        }

        /// <summary>
        /// Disposes the current instance of the <see cref="MsSqlFileSystem"/>.
        /// </summary>
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
