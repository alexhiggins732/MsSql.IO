using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem;


using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace FtpServer.MsSqlFileSystem
{
    /// <summary>
    /// Extension methods for <see cref="IFtpServerBuilder"/> using to configure the <see cref="MsSqlFileSystem"/> using Dependency Injection.
    /// </summary>
    public static class MsSqlFtpServerBuilderExtensions
    {
        /// <summary>
        /// Registers a <see cref="MsSqlFileSystemProvider"/> implementation of the <see cref="IFileSystemClassFactory"/> to provide a <see cref="MsSqlFileSystem"/> API.
        /// </summary>
        /// <param name="builder">The server builder used to configure the FTP server.</param>
        /// <returns>the server builder used to configure the FTP server.</returns>
        public static IFtpServerBuilder UseMsSqlFileSystem(this IFtpServerBuilder builder)
        {
            builder.Services.AddSingleton<IFileSystemClassFactory, MsSqlFileSystemProvider>();
            return builder;
        }
    }
}
