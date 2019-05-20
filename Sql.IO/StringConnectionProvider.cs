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

    public class ConfigurationConnectionStringProvider: StringConnectionProvider
    {
        public ConfigurationConnectionStringProvider():base(GetConfigurationConfigurationString()) { }

        private static string GetConfigurationConfigurationString()
        {
            var connectionStringKey = ConfigurationManager.AppSettings[DbConstants.SqlContextConnectionName];
            if (connectionStringKey is null)
                throw new Exception($"{DbConstants.SqlContextConnectionName} has not been configured not specified in application configuration");
            return ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;
        }
        public static ConfigurationConnectionStringProvider Instance => new ConfigurationConnectionStringProvider();
    }
}


