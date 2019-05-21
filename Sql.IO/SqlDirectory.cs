using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IO
{
    /// <summary>
    /// Helper extensions to extend commonly used <see cref="Directory"/> methods to a <see cref="SqlDirectoryInfo"/>.
    /// </summary>
    public static class SqlDirectory
    {
        /// <summary>
        /// Creates a new directory at the specified path and returns a <see cref="SqlDirectoryInfo"/> for the directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SqlDirectoryInfo CreateDirectory(string path)
        {
            var result = new SqlDirectoryInfo(path);
            result.Create();
            return result;
        }

        /// <summary>
        /// Moves a file or a directory and its contents to a new location.
        /// </summary>
        /// <param name="srcPath">The path to the file or directory to be moved.</param>
        /// <param name="destinationPath">The path to move the content at the source to. 
        /// If the source if a file then destinationPath must also be a file name. 
        /// If the source is a directory then destinationPath must also be a directory.
        /// </param>
        public static void Move(string srcPath, string destinationPath)
        {
            var entry = SqlPath.GetFileSystemInfo(srcPath);
            if (!entry.Is_Directory)
                ((SqlFileInfo)entry).MoveTo(destinationPath);
            else
                ((SqlDirectoryInfo)entry).MoveTo(destinationPath);
        }

        /// <summary>
        /// Deletes thew directory at the specified path. If the directory is not empty an exception is thrown. 
        /// To delete a directory and all of its contents use <see cref="SqlDirectory.Delete(string, bool)"/>.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        /// <returns></returns>
        public static void Delete(string path) => new SqlDirectoryInfo(path).Delete();

        /// <summary>
        /// Deletes thew directory at the specified path. If the directory is not empty an exception is thrown.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        /// <returns></returns>
        public static void Delete(string path, bool recursive) => new SqlDirectoryInfo(path).Delete(recursive);

        /// <summary>
        /// Returns a list of <see cref="SqlDirectory"/> located in the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<SqlDirectoryInfo> EnumerateDirectories(string path) => new SqlDirectoryInfo(path).GetDirectories();

        /// <summary>
        /// Returns a list of <see cref="SqlFileInfo"/> located in the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<SqlFileInfo> EnumerateFiles(string path) => new SqlDirectoryInfo(path).GetFiles();

        /// <summary>
        /// Returns a list of <see cref="SqlFileInfo"/> located in the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<SqlFileSystemInfo> EnumerateFileSystemInfo(string path) => SqlPath.GetFileSystemEntries(path);

        /// <summary>
        /// Returns true if a directory exists at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns true if a directory exists at the specified path.</returns>
        public static bool Exists (string path) => new SqlDirectoryInfo(path).Exists;

        /// <summary>
        /// Returns the parent of the directory at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SqlDirectoryInfo GetParent(string path) => new SqlDirectoryInfo(path).Directory;
    }
}
