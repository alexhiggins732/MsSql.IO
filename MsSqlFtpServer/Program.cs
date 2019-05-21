using FtpServer.MsSqlFileSystem;
using FubarDev.FtpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Sql.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MsSqlFtpServer
{
    /// <summary>
    /// Configures Dependency Injection to use the <see cref="MsSqlFileSystem"/> and starts a <see cref="FtpServerHost"/>
    /// </summary>
    partial class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CreateFtpHostBuilderAndRun(args);//.Build().Run();
        }

        /// <summary>
        /// Configures Dependency Injection to start a <see cref="FtpServerHost"/> using the <see cref="MsSqlFileSystem"/>
        /// </summary>
        /// <param name="args"></param>
        public static void CreateFtpHostBuilderAndRun(string[] args)
        {
            var services = CreateServices(new FtpServerConfigOptions());

            var uncInfo = UncInfo.Default().ToString();

            services.Configure<MsSqlFileSystemOptions>(opt => opt
                .RootPath = uncInfo);

            // Add FTP server services
            // MsSqlFileSystemProvider = Uses the MS SQL file system functionality
            // AnonymousMembershipProvider = allow only anonymous logins got noe
            services.AddFtpServer(builder => builder
                .UseMsSqlFileSystem() // Use the .NET file system functionality
                .EnableAnonymousAuthentication()); // allow anonymous logins


            // Configure the FTP server
            services.Configure((FubarDev.FtpServer.FtpServerOptions opt) => opt.ServerAddress = "127.0.0.1");

            // And run it.
            Task.Run(() => RunAsync(services)).Wait();

        }

        /// <summary>
        /// Builds a Service Provider and starts the <see cref="FtpServerHost"/>.
        /// </summary>
        /// <param name="services">The collection of services to provide for Dependency Injection.</param>
        /// <returns></returns>
        private static async Task RunAsync(IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                NLog.LogManager.LoadConfiguration("NLog.config");

                try
                {
                    // Start the FTP server
                    var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();
                    await ftpServerHost.StartAsync(CancellationToken.None).ConfigureAwait(false);

                    Console.WriteLine("Press ENTER/RETURN to close the test application.");
                    Console.ReadLine();

                    // Stop the FTP server
                    await ftpServerHost.StopAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Configures default Dependency Injection Services
        /// </summary>
        /// <param name="options"><see cref="FtpServerConfigOptions"/> options for the <see cref="FtpServer"/></param>
        /// <returns></returns>
        private static IServiceCollection CreateServices(FtpServerConfigOptions options)
        {
            var services = new ServiceCollection()
                .AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Trace))
                .AddOptions()
                .Configure<AuthTlsOptions>(
                    opt =>
                    {
                        if (options.ServerCertificateFile != null)
                        {
                            opt.ServerCertificate = new X509Certificate2(
                                options.ServerCertificateFile,
                                options.ServerCertificatePassword);
                        }
                    })
                .Configure<FtpConnectionOptions>(opt => opt.DefaultEncoding = Encoding.ASCII)
                .Configure<FtpServerOptions>(
                    opt =>
                    {
                        opt.ServerAddress = options.ServerAddress;
                        opt.Port = options.GetPort();

                        if (options.PassivePortRange != null)
                        {
                            //opt.PasvMinPort = options.PassivePortRange.Value.Item1;
                            //opt.PasvMaxPort = options.PassivePortRange.Value.Item2;
                        }
                    })
                ;

            if (options.ImplicitFtps)
            {
                services.Decorate<IFtpServer>(
                    (ftpServer, serviceProvider) =>
                    {
                        var authTlsOptions = serviceProvider.GetRequiredService<IOptions<AuthTlsOptions>>();

                        // Use an implicit SSL connection (without the AUTHTLS command)
                        ftpServer.ConfigureConnection += (s, e) =>
                        {
                            var sslStream = new SslStream(e.Connection.OriginalStream);
                            sslStream.AuthenticateAsServer(authTlsOptions.Value.ServerCertificate);
                            e.Connection.SocketStream = sslStream;
                        };

                        return ftpServer;
                    });
            }

            return services;
        }




    }

}
