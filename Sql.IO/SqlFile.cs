using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IO
{
    /// <summary>
    /// Helper extensions to extend commonly used <see cref="File"/> methods to a <see cref="SqlFileInfo"/>'s underlying stream
    /// </summary>
    public static class SqlFile
    {
        /// <summary>
        /// The default Windows-1252 Code Page
        /// </summary>
        private static Encoding WindowsEncoding = Encoding.GetEncoding(1252);

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/> as string array.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        public static string[] ReadAllLines(this SqlFileTable fileTable, Guid stream_Id)
            => fileTable.GetFile(stream_Id).ReadAllLines();

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/> as string array.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        public static string[] ReadAllLines(this SqlFileInfo sqlFileInfo)
        {
            var result = new List<string>();
            using (var sr = new StreamReader(sqlFileInfo.File_Stream()))
                while (!sr.EndOfStream)
                    result.Add(sr.ReadLine());
            return result.ToArray();
        }

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/>'s underlying stream as string.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        public static string ReadAllText(this SqlFileTable fileTable, Guid stream_Id)
             => fileTable.GetFile(stream_Id).ReadAllText();

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/>'s underlying stream as string.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        public static string ReadAllText(this SqlFileInfo sqlFileInfo)
        {
            using (var sr = new StreamReader(sqlFileInfo.File_Stream()))
                return sr.ReadToEnd();
        }

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/>'s underlying stream as a byte array using Windows-1252 <see cref="Encoding"/>.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this SqlFileTable fileTable, Guid stream_Id)
            => fileTable.GetFile(stream_Id).ReadAllBytes(WindowsEncoding);

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/>'s underlying stream as a byte array using Windows-1252 <see cref="Encoding"/>.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this SqlFileInfo sqlFileInfo)
            => ReadAllBytes(sqlFileInfo, WindowsEncoding);

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/>'s underlying stream as a byte array using Windows-1252 <see cref="Encoding"/>.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this SqlFileTable fileTable, Guid stream_Id, Encoding encoding)
            => fileTable.GetFile(stream_Id).ReadAllBytes(encoding);

        /// <summary>
        /// Reads the contents of the <see cref="SqlFileInfo"/>'s underlying stream as a byte array using the specified <see cref="Encoding"/>.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this SqlFileInfo sqlFileInfo, Encoding encoding)
        {
            using (var br = new BinaryReader(sqlFileInfo.File_Stream(), encoding))
                return br.ReadBytes((int)br.BaseStream.Length);
        }

        /// <summary>
        /// Writes the specified string array to the <see cref="SqlFileInfo"/>'s underlying stream. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllLines(this SqlFileTable fileTable, Guid stream_Id, string[] value)
            => fileTable.GetFile(stream_Id).WriteAllLines(value);

        /// <summary>
        /// Writes the specified string array to the <see cref="SqlFileInfo"/>'s underlying stream. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllLines(this SqlFileInfo sqlFileInfo, string[] value)
        {
            using (var sw = new StreamWriter(sqlFileInfo.Create()))
                foreach (var line in value)
                    sw.WriteLine(line);
        }

        /// <summary>
        /// Writes the specified byte array to the <see cref="SqlFileInfo"/>'s underlying stream using Windows-1252 <see cref="Encoding"/>. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllBytes(this SqlFileTable fileTable, Guid stream_Id, byte[] value)
            => fileTable.GetFile(stream_Id).WriteAllBytes(value, WindowsEncoding);

        /// <summary>
        /// Writes the specified byte array to the <see cref="SqlFileInfo"/>'s underlying stream using Windows-1252 <see cref="Encoding"/>. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllBytes(this SqlFileInfo sqlFileInfo, byte[] value)
            => WriteAllBytes(sqlFileInfo, value, WindowsEncoding);

        /// <summary>
        /// Writes the specified byte array to the <see cref="SqlFileInfo"/>'s underlying stream using the specified <see cref="Encoding"/>. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllBytes(this SqlFileTable fileTable, Guid stream_Id, byte[] value, Encoding encoding)
            => fileTable.GetFile(stream_Id).WriteAllBytes(value, encoding);

        /// <summary>
        /// Writes the specified byte array to the <see cref="SqlFileInfo"/>'s underlying stream using the specified <see cref="Encoding"/>. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllBytes(this SqlFileInfo sqlFileInfo, byte[] value, System.Text.Encoding encoding)
        {
           
            using (var br = new BinaryWriter(sqlFileInfo.Create(), encoding))
                br.Write(value);
        }

        /// <summary>
        /// Writes the specified string to the <see cref="SqlFileInfo"/>'s underlying stream. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllText(this SqlFileTable fileTable, Guid stream_Id, string value)
            => fileTable.GetFile(stream_Id).WriteAllText(value);

        /// <summary>
        /// Writes the specified string to the <see cref="SqlFileInfo"/>'s underlying stream. 
        /// If the file exists, it's content is overwritten.
        /// </summary>
        /// <param name="sqlFileInfo"></param>
        /// <param name="value"></param>
        public static void WriteAllText(this SqlFileInfo sqlFileInfo, string value)
        {
            using (var sw = new StreamWriter(sqlFileInfo.Create()))
                sw.Write(value);
        }

        /// <summary>
        /// Creates a new empty file at the specified locatoin and returns the <see cref="SqlFileInfo"/> for the file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SqlFileInfo Create(string path)
        {
            var result = new SqlFileInfo(path);
            result.Create().Close();
            return result;
        }

        /// <summary>
        /// Moves a file from the specified source path to the specified destination path
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        public static void Move(string sourcePath, string destinationPath) => new SqlFileInfo(sourcePath).MoveTo(destinationPath);

        /// <summary>
        /// Returns true if a <see cref="SqlFile>"/> exists at the specified path.
        /// </summary>
        /// <param name="sourcePath">The path to check if the file exists</param>
        /// <returns>Returns true if a <see cref="SqlFile"/> exists at the specified path.</returns>
        public static bool Exists(string sourcePath) => new SqlFileInfo(sourcePath).Exists;

        /// <summary>
        /// Deletes the file at the specified path
        /// </summary>
        /// <param name="sourcePath">The path to the file to delete.</param>
        public static void Delete(string sourcePath) => new SqlFileInfo(sourcePath).Delete();
    }
}
