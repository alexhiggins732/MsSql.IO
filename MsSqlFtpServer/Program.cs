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
    class Program
    {
        static void Main(string[] args)
        {
            CreateFtpHostBuilder(args).Build().Run();
        }


        public interface IFtpHostBuilder
        {
            IFtpHost Build();
        }
        public interface IFtpHost
        {
            bool Running { get; }
            void Run();
            void Stop();
        }
        public class FtpHost : IFtpHost
        {
            private IFtpServerHost ftpServerHost;
            private IServiceProvider serviceProvider;
            public FtpHost(IServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
                ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();
            }

            public bool Running { get; private set; }

            public void Run()
            {
                if (Running) return;
                ftpServerHost.StartAsync(CancellationToken.None).Wait();
                //Console.ReadLine();
                Running = true;
                Task.Run(() => PromptForExit());
            }

            private void PromptForExit()
            {
                Console.ReadKey();
                Stop();
            }

            public void Stop()
            {
                if (!Running) return;
                ftpServerHost.StopAsync(CancellationToken.None).Wait();
                Running = false;
            }
        }

        public class FtpHostBuilder : IFtpHostBuilder
        {
            private ServiceProvider serviceProvider;

            public FtpHostBuilder(ServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
            }

            public IFtpHost Build()
            {
                //ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();
                var iFtpHost = serviceProvider.GetRequiredService<IFtpHost>();
                return iFtpHost;
                //return new FtpHost(ftpServerHost);
            }
        }
        public static IFtpHostBuilder CreateFtpHostBuilder(string[] args)
        {
            var services = CreateServices(new FtpServerConfigOptions());

             var uncInfo = UncInfo.Default().ToString();

            services.Configure<MsSqlFileSystemOptions>(opt => opt
                .RootPath = uncInfo); // Path.Combine(Path.GetTempPath(), "TestFtpServer"));

            // Add FTP server services
            // DotNetFileSystemProvider = Use the  DotNet file system functionality
            // AnonymousMembershipProvider = allow only anonymous logins
            services.AddFtpServer(builder => builder
                .UseMsSqlFileSystem() // Use the .NET file system functionality
                .EnableAnonymousAuthentication()); // allow anonymous logins

            // Configure the FTP server
            services.Configure((FubarDev.FtpServer.FtpServerOptions opt) => opt.ServerAddress = "127.0.0.1");
            services.AddSingleton<IFtpHost>(x => new FtpHost(x));
            Task.Run(() => RunAsync(services)).Wait();
            //return new FtpHostBuilder(services.BuildServiceProvider());
            //Build the service provider
            using (var serviceProvider = services.BuildServiceProvider())
            {

                // Initialize the FTP server
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

                //// Start the FTP server
                ftpServerHost.StartAsync(CancellationToken.None).Wait();

                Console.WriteLine("Press ENTER/RETURN to close the test application.");
                Console.ReadLine();

                //// Stop the FTP server
                ftpServerHost.StopAsync(CancellationToken.None).Wait();
            }
            return new FtpHostBuilder(services.BuildServiceProvider());
        }

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


    public class FtpServerConfigOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the help message should be shown.
        /// </summary>
        public bool ShowHelp { get; set; }

        /// <summary>
        /// Gets or sets the requested server address.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the requested FTP server port.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the FTP server should use implicit FTPS.
        /// </summary>
        public bool ImplicitFtps { get; set; }

        /// <summary>
        /// Gets or sets the path to the server certificate file.
        /// </summary>
        public string ServerCertificateFile { get; set; }

        /// <summary>
        /// Gets or sets the password of the server certificate file.
        /// </summary>
        public string ServerCertificatePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the in-memory file system should be kept between two connects.
        /// </summary>
        public bool KeepAnonymousInMemoryFileSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Google Drive access token should be refreshed.
        /// </summary>
        public bool RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether background upload should be used.
        /// </summary>
        public bool UseBackgroundUpload { get; set; }

        /// <summary>
        /// Gets or sets the membership provider to be used.
        /// </summary>
        public MembershipProviderType MembershipProviderType { get; set; } = MembershipProviderType.Anonymous;

        /// <summary>
        /// Gets or sets the passive port range.
        /// </summary>
        public (int, int)? PassivePortRange { get; set; }

        /// <summary>
        /// Gets the requested or the default port.
        /// </summary>
        /// <returns></returns>
        public int GetPort()
        {
            return Port ?? (ImplicitFtps ? 990 : 21);
        }

        /// <summary>
        /// Validates the current configuration.
        /// </summary>
        public void Validate()
        {
            if (ImplicitFtps && !string.IsNullOrEmpty(ServerCertificateFile))
            {
                throw new Exception("Implicit FTPS requires a server certificate.");
            }
        }
    }
    /// <summary>
    /// The selected membership provider.
    /// </summary>
    public enum MembershipProviderType
    {
        /// <summary>
        /// Use the custom (example) membership provider.
        /// </summary>
        Custom,

        /// <summary>
        /// Use the membership provider for anonymous users.
        /// </summary>
        Anonymous,
    }

}
