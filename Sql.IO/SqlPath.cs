using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace Sql.IO
{

    /// <summary>
    /// Utility class for parsing <see cref="SqlPathInfo"/> from file paths;
    /// </summary>
    public class SqlPath
    {
        /// <summary>
        /// Parses the <see cref="SqlPathInfo.UncRoot"/> segment of a path.
        /// </summary>
        /// <param name="path">The UNC path to parse.</param>
        /// <returns></returns>
        public static string GetUncRoot(string path)
            => SqlPathInfo.Parse(path).UncRoot;

        /// <summary>
        /// Parses the <see cref="SqlPathInfo.FileStreamDirectory"/> segment of a path.
        /// </summary>
        /// <param name="path">The UNC path to parse.</param>
        /// <returns></returns>
        public static string GetFileStreamDirectory(string path)
            => SqlPathInfo.Parse(path).FileStreamDirectory;

        /// <summary>
        /// Parses the <see cref="SqlPathInfo.FileTableDirectory"/> segment of a path.
        /// </summary>
        /// <param name="path">The UNC path to parse.</param>
        /// <returns></returns>
        public static string GetFileTableDirectory(string path)
            => SqlPathInfo.Parse(path).FileTableDirectory;


        /// <summary>
        /// Parses the <see cref="SqlPathInfo.RelativePath"/> segment of a path.
        /// </summary>
        /// <param name="path">The UNC path to parse.</param>
        /// <returns></returns>
        public static string GetFileTableRelativePath(string path)
            => SqlPathInfo.Parse(path).RelativePath;

        /// <summary>
        /// Deterimines if the specified path contains a root.
        /// </summary>
        /// <param name="path">The UNC path to parse.</param>
        /// <returns></returns>
        public bool IsPathRooted(string path) => System.IO.Path.IsPathRooted(path);

        /// <summary>
        /// Gets the root directory information from the specified path.
        /// </summary>
        /// <param name="path">The path to get the root directory information from</param>
        /// <returns></returns>
        public string GetRootPath(string path) => System.IO.Path.GetPathRoot(path);

        /// <summary>
        /// Gets a <see cref="SqlFileSystemInfo"/> for the specified path.
        /// </summary>
        /// <param name="path">The path to the <see cref="SqlFileSystemInfo"/> </param>
        /// <returns></returns>
        public static SqlFileSystemInfo GetFileSystemInfo(string path)
        {

            var info = SqlPathInfo.Parse(path);
            var provider = SqlContext.GetConnectionStringProviderForPath(path);

            var fileTable = SqlFileTable.GetSqlFileTable(provider, info.FileTableDirectory);

            //TODO: Cleanup embedded T-SQL
            var sql = $@"
SELECT 
 {DbConstants.FileTableSelectList}
FROM 
    [{fileTable.Table_Name}]
where file_stream.GetFileNamespacePath() = @RelativePath
";
            //TODO: Isolate database access
            using (var conn = new SqlConnection(provider.ConnectionString))
            {
                conn.Open();
                var entry = conn.QueryFirstOrDefault<SqlFileSystemEntry>(sql, new { info.RelativePath });
                if (entry.Stream_Id != Guid.Empty)
                {
                    if (entry.Is_Directory)
                    {
                        return new SqlDirectoryInfo(entry, provider, fileTable);
                    }
                    else
                    {
                        return new SqlFileInfo(entry, provider, fileTable);
                    }
                }
            }
            return null;

        }

        public static List<SqlFileSystemInfo> GetFileSystemEntries(string path)
        {

            var info = SqlPathInfo.Parse(path);

            var provider = SqlContext.GetConnectionStringProviderForPath(info);

            var fileTable = SqlFileTable.GetSqlFileTable(provider, info.FileTableDirectory);

            var sql = "";
            if (info.IsFileTableDirectory)
            {
                sql = $@"
SELECT 
 {DbConstants.FileTableSelectList}
FROM 
    [{fileTable.Table_Name}]
where [parent_path_locator] is null
";
            }
            else
            {
                //TODO: Cleanup embedded T-SQL
                sql = $@"
SELECT 
 {DbConstants.FileTableSelectList}
FROM 
    [{fileTable.Table_Name}]
where [parent_path_locator].ToString() = 
(select top 1 path_locator from [{fileTable.Table_Name}] where file_stream.GetFileNamespacePath()= @RelativePath)
";
            }
            List<SqlFileSystemInfo> result = null;

            //TODO: Isolate database access
            using (var conn = new SqlConnection(provider.ConnectionString))
            {
                conn.Open();
                result = conn.Query<SqlFileSystemEntry>(sql, new { info.RelativePath })
                    .Select(x => x.Is_Directory ? (SqlFileSystemInfo)new SqlDirectoryInfo(x, provider, fileTable) : (SqlFileSystemInfo)new SqlFileInfo(x, provider, fileTable))
                    .ToList();

            }
            return result;

        }

    }
}
