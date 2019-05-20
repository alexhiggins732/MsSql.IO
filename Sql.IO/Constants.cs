using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IO
{
    public class Constants
    {
        public const string BackslashString = @"\";
        public const char PeriodChar = '.';
        public const char BackslashChar = '\\';
        public static readonly char[] BackslashChars = new[] { BackslashChar };
        public const string EntryIsNotFile = "Entry is not a file.";
        public const string EntryIsNotDirectory = "Entry is not a file.";
        public const string ParentDirectoryDoesNotExist = "The parent directory for this file does not exist.";
        public const string DirectoryDoesNotExist = "The directory does not exist.";
        public const string FileDoesNotExists = "The file does not exist";
        public const string SqlLocatorIdFormat = "{0}.{1}.{2}";

        public const string PathMustBeAbsoluteUnc = "Path must be an absolute UNC path.";
        public static readonly string PathMustStartWithUncRoot = $@"The path must begin with a UNC root directoryg pointing to \\{nameof(SqlPathInfo.ServerName)}\{nameof(SqlPathInfo.InstanceName)}";
        public const string PathMissingFileStreamDirectoryBackslash = "Path format is invalid. Expected backslash not found attempting to parse File Stream directory.";
        public const string PathMissingFileTableBackslash = "Path format is invalid. Expected backslash not found attempting to parse File Table directory.";
        public const string PathMissingRelativePathBackslash = "Path format is invalid. Expected backslash not found attempting to parse relative path.";
    }
}
