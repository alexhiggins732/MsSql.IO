using System;
using System.Configuration;

namespace Sql.IO
{
    /// <summary>
    /// Concreted implementation of the <see cref="IConnectionStringProvider"/> allowing 
    /// a <see cref="System.Data.IDbConnection.ConnectionString"/> to be resolved using Dependency Injection.
    /// </summary>
    public class StringConnectionProvider : IConnectionStringProvider
    {
        /// <summary>
        /// The string that is used to open a connection to the database. See: <see cref="System.Data.IDbConnection.ConnectionString"/>
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Initialzes a new <see cref="StringConnectionProvider"/>.
        /// </summary>
        /// <param name="connectionString">The used to open a connection to the database.</param>
        public StringConnectionProvider(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
    }

    /// <summary>
    /// Returns a connection string initialize from an application config file.
    /// </summary>
    public class ConfigurationConnectionStringProvider: StringConnectionProvider
    {
        /// <summary>
        /// Returns a new <see cref="ConfigurationConnectionStringProvider"/>
        /// </summary>
        public ConfigurationConnectionStringProvider():base(GetConfigurationConfigurationString()) { }

        /// <summary>
        /// Returns the connection string specified in the application config file.
        /// </summary>
        /// <returns></returns>
        private static string GetConfigurationConfigurationString()
        {
            var connectionStringKey = ConfigurationManager.AppSettings[DbConstants.SqlContextConnectionName];
            if (connectionStringKey is null)
                throw new Exception($"{DbConstants.SqlContextConnectionName} has not been configured not specified in application configuration");
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringKey];
            if (connectionStringSettings is null)
                throw new Exception($"A connection string with the name '{connectionStringKey}' has not been configured");
            return connectionStringSettings.ConnectionString;

        }
        /// <summary>
        /// Returns a default instance of the <see cref="ConfigurationConnectionStringProvider"/>
        /// </summary>
        public static ConfigurationConnectionStringProvider Instance => new ConfigurationConnectionStringProvider();
    }
}


