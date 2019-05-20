using Dapper;
using Sql.IO.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Sql.IO
{
    public class SqlContext
    {
        //TODO: DI SqlContext.
        public static SqlContext Current = new SqlContext(GetRequiredService<IConnectionStringProvider>());
        private static IFileTableQueryProvider queryProvider;

        //TODO: Implement DI Services. This is just a stub for development purposes
        private static T GetRequiredService<T>()
        {
            if (typeof(T) == typeof(IConnectionStringProvider))
            {
                return (T)(object)new ConfigurationConnectionStringProvider();
            }
            else if (typeof(T) == typeof(IFileTableQueryProvider))
            {
                return (T)(object)new FileTableQueryProvider();
            }

            throw new NotImplementedException();
        }
        /// <summary>
        /// The connection string for the <see cref="SqlContext"/> instance;
        /// </summary>
        public string ConnectionString { get; set; }

        public IConnectionStringProvider ConnectionStringProvider { get; set; }
        public SqlContext(IConnectionStringProvider connectionStringProvider)
        {
            this.ConnectionStringProvider = connectionStringProvider;
            this.ConnectionString = connectionStringProvider.ConnectionString; 
        }

        /// <summary>
        /// Returns the <see cref="IConnectionStringProvider"/> used to connection to the datbase containing the File Table for the specified path.
        /// </summary>
        /// <param name="path">A string containing <see cref="SqlPathInfo"/> to lookup the <see cref="IConnectionStringProvider"/> for.</param>
        /// <returns></returns>
        public static IConnectionStringProvider GetConnectionStringProviderForPath(string path)
            => ConfigurationConnectionStringProvider.Instance;

        /// <summary>
        /// Returns the <see cref="IConnectionStringProvider"/> used to connection to the datbase containing the File Table for the specified path.
        /// </summary>
        /// <param name="pahtInfo">The <see cref="SqlPathInfo"/> to lookup the <see cref="IConnectionStringProvider"/> for.</param>
        /// <returns></returns>
        public static IConnectionStringProvider GetConnectionStringProviderForPath(SqlPathInfo pahtInfo)
            => ConfigurationConnectionStringProvider.Instance;

        private List<T> Get<T>(string sql, object param)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                return conn.Query<T>(sql, param).AsList();
            }
        }
        internal List<SqlFileSystemEntry> GetChildDirectories(string table_Name, object param)
        {
            var tsql = queryProvider.ChildDirectoriesQuery(table_Name);
            return Get<SqlFileSystemEntry>(tsql, param);
        }
    }
    public interface IFileTableQueryProvider
    {
        string ChildDirectoriesQuery(string table_Name);
    }

    public class FileTableQueryProvider : IFileTableQueryProvider
    {
        public string ChildDirectoriesQuery(string table_Name)
        {
            var queryFormat = Resources.GetChildDirectoriesQueryFormat;
            return string.Format(queryFormat, DbConstants.FileTableSelectList, table_Name, DbConstants.PathLocatorParameterName);
        }
    }
}
