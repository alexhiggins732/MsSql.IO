using System.IO;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;
using FubarDev.FtpServer.FileSystem;

namespace FtpServer.MsSqlFileSystem
{
    /// <summary>
    /// A <see cref="IFileSystemClassFactory"/> factory implementation 
    /// to <see cref="MsSqlFileSystem"/> functionality for file system access.
    /// </summary>
    public class MsSqlFileSystemProvider : IFileSystemClassFactory
    {
        private readonly string _rootPath;

        private readonly bool _useUserIdAsSubFolder;

        private readonly int _streamBufferSize;

        private readonly bool _allowNonEmptyDirectoryDelete;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The file system options.</param>
        public MsSqlFileSystemProvider([NotNull] IOptions<MsSqlFileSystemOptions> options)
        {
            _rootPath = options.Value.RootPath ?? throw new System.ArgumentException("Root path to SQL Server Share must be specified");
            _useUserIdAsSubFolder = options.Value.UseUserIdAsSubFolder;
            _streamBufferSize = options.Value.StreamBufferSize ?? MsSqlFileSystem.DefaultStreamBufferSize;
            _allowNonEmptyDirectoryDelete = options.Value.AllowNonEmptyDirectoryDelete;
        }


        /// <summary>
        /// Returns a new <see cref="MsSqlFileSystem"/> instance implementation of the <see cref="IUnixFileSystem"/>.
        /// </summary>
        /// <param name="userId">The userId of the current user.</param>
        /// <param name="isAnonymous">A flag indicating if the login is anonymous.</param>
        /// <returns>Returns a new <see cref="MsSqlFileSystem"/> instance.</returns>
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var path = _rootPath;
            if (_useUserIdAsSubFolder)
            {
                path = Path.Combine(path, userId);
            }

            return Task.FromResult<IUnixFileSystem>(new MsSqlFileSystem(path, _allowNonEmptyDirectoryDelete, _streamBufferSize));
        }
    }
}

