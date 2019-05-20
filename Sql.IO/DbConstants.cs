using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IO
{
    public class DbConstants
    {
        public const string FileTableSelectList = @"
	[stream_id]
    ,[name]
    ,[path_locator].ToString() as [path_locator]
    ,[parent_path_locator].ToString() as [parent_path_locator]
    ,[file_type]
    ,[cached_file_size]
    ,[creation_time]
    ,[last_write_time]
    ,[last_access_time]
    ,[is_directory]
    ,[is_offline]
    ,[is_hidden]
    ,[is_readonly]
    ,[is_archive]
    ,[is_system]
    ,[is_temporary]
	, FileTableRootPath() + file_stream.GetFileNamespacePath() as [FullName]
";

        public const string SqlContextConnectionName = nameof(SqlContextConnectionName);
        public const string StreamIdParameterName = "@stream_Id";
        public const string RelativePathParameterName = "@RelativePath";
        public const string DirectoryNameParameterName = "@directoryName";
        public const string PathLocatorParameterName = " @path_Locator";
        public const string NameParameterName = "@name";
    }
}
