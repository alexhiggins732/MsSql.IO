using System;
using System.Collections.Generic;

namespace Sql.IO
{

    /// <summary>
    /// A utililty class for parsing the logical directory heirarchy from a supplied path
    /// </summary>
    public class SqlPathInfo
    {

        /// <summary>
        /// The original path supplied to the parser.
        /// </summary>
        public string OriginalPath { get; private set; }
        
        /// <summary>
        /// The Machine Name or Virtual Network Name of the SQL server parsed from the supplied path.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// The Instance name of the Sql Server parsed from the supplied path.
        /// </summary>
        public string InstanceName { get; private set; }

        /// <summary>
        /// If present, the name of the File Stream directory parsed from the supplied path.
        /// </summary>
        public string FileStreamDirectory { get; private set; }

        /// <summary>
        /// If present, the name of the FileTable directory parsed from the supplied path.
        /// </summary>
        public string FileTableDirectory { get; private set; }
        /// <summary>
        /// If present, the absolute path, relative to <see cref="FileStreamDirectory"/>, parsed from the supplied path.
        /// </summary>
        public string RelativePath { get; private set; }

        /// <summary>
        /// The root UNC directory parse from the supplied path. The will be the \\ServerName\InstanceName part of the path.
        /// </summary>
        public string UncRoot { get; private set; }

        /// <summary>
        /// Parses <see cref="SqlPathInfo"/> from the specified UNC path. At a minimum the path must contain a valid <see cref="UncRoot"/> or an error will be thrown. 
        /// The <see cref="FileStreamDirectory"/>, <see cref="FileTableDirectory"/> and <see cref="RelativePath"/> are also parsed if present.
        /// </summary>
        /// <remarks>For details specs on path formats, see: https://docs.microsoft.com/en-us/sql/relational-databases/blob/work-with-directories-and-paths-in-filetables?view=sql-server-2017</remarks>
        public static SqlPathInfo Parse(string path)
        {
            var result = new SqlPathInfo();
            result.OriginalPath = path;

            //nomalize and validate the supplied path
            path = System.IO.Path.GetFullPath(path);

            var uncRoot = System.IO.Path.GetPathRoot(path);
            if (uncRoot == Constants.BackslashString || !uncRoot.StartsWith(Constants.BackslashString))
            {
                throw new ArgumentException(Constants.PathMustBeAbsoluteUnc, nameof(path)); 
            } 
            result.UncRoot = uncRoot;
            var dbInfo = result.UncRoot.Substring(2);
            var idx = dbInfo.IndexOf(Constants.BackslashChar);
            if (idx == -1)
                throw new UriFormatException(Constants.PathMustStartWithUncRoot); 

            result.ServerName = dbInfo.Substring(0, dbInfo.IndexOf(Constants.BackslashChar));
            result.InstanceName = dbInfo.Substring(dbInfo.IndexOf(Constants.BackslashChar) + 1);

            path = path.Substring(uncRoot.Length);
            if (path.Length > 0)
            {
                if (!path.StartsWith(Constants.BackslashString))
                    throw new ArgumentException(Constants.PathMissingFileStreamDirectoryBackslash);

                path = path.Substring(1);
                idx = path.IndexOf(Constants.BackslashString);

                if (idx == -1)
                {
                    result.FileStreamDirectory = path;
                }
                else
                {
                    result.FileStreamDirectory = path.Substring(0, idx);
                    path = path.Substring(result.FileStreamDirectory.Length);
                    if (path.Length > 0)
                    {
                        if (!path.StartsWith(Constants.BackslashString))
                            throw new ArgumentException(Constants.PathMissingFileTableBackslash); 

                        path = path.Substring(1);
                        idx = path.IndexOf(Constants.BackslashString);

                        if (idx != -1)
                        {
                            result.FileTableDirectory = path.Substring(0, idx);

                            path = path.Substring(result.FileTableDirectory.Length);

                            if (path.Length > 0)
                            {
                                if (!path.StartsWith(Constants.BackslashString))
                                    throw new ArgumentException(Constants.PathMissingRelativePathBackslash); 
                                path = path.Substring(1);

                                if (path.Length > 0)
                                {
                                    result.RelativePath = Constants.BackslashString + result.FileTableDirectory + Constants.BackslashString + path.TrimEnd(Constants.BackslashChars);
                                }
                                else
                                {
                                    result.RelativePath = Constants.BackslashString + result.FileTableDirectory;
                                }
                            }
                            else
                            {
                                result.RelativePath = Constants.BackslashString + result.FileTableDirectory;
                            }
                        }
                        else
                        {
                            result.FileTableDirectory = path;
                            result.RelativePath = Constants.BackslashString + path;
                        }
                    }
                }
            }
            return result;
        }

    }

  
}
