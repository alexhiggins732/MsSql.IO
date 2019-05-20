using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.IO;

namespace Sql.IO
{
    /// <summary>
    /// A <see cref="StreamWrapper"/> to provide functionality to work with a Sql FileStream as if it were a regular <see cref="System.IO.FileStream"/>.
    /// </summary>
    public class SqlFileStream : StreamWrapper, IDisposable
    {
        //TODO: Document SqlFileStream members;
        private string fileTableName;
        private Guid stream_Id;
        private string connectionString;
        private string tempFileName;
        private FileMode fileMode;
        public SqlFileStream(Guid Stream_Id, string FileTableName, string connectionString)
            : base()
        {
            GetSqlFileStream(Stream_Id, FileTableName, connectionString);
        }

        public SqlFileStream(Guid Stream_Id, SqlFileTable fileTable)
            : base()
        {
            GetSqlFileStream(Stream_Id, fileTable.Table_Name, fileTable.connectionStringProvider.ConnectionString);
        }

        public SqlFileStream(Guid Stream_Id, SqlFileTable fileTable, FileMode fileMode)
            : base()
        {
            this.fileMode = fileMode;
            GetSqlFileStream(Stream_Id, fileTable.Table_Name, fileTable.connectionStringProvider.ConnectionString);
        }


        private void GetSqlFileStream(Guid stream_Id, string fileTableName, string connectionString)
        {
            this.fileTableName = fileTableName;
            this.stream_Id = stream_Id;
            this.connectionString = connectionString;
            Stream result = null;

            //TODO: Cleanup embedded T-SQL
            var commandText = $"SELECT file_stream.PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() FROM [{fileTableName}] WHERE stream_id={DbConstants.StreamIdParameterName}";

            //TODO: Isolate database access
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var trans = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                var command = new SqlCommand(commandText, connection);
                command.Transaction = trans;
                command.Parameters.AddWithValue(DbConstants.StreamIdParameterName, stream_Id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string path = reader.GetString(0);
                    byte[] transactionContext = reader.GetSqlBytes(1).Buffer;
                    var access = fileMode == FileMode.CreateNew ? FileAccess.Write : FileAccess.Read;
                    using (var stream = new System.Data.SqlTypes.SqlFileStream(path, transactionContext, access, FileOptions.SequentialScan, allocationSize: 0))
                    {
                        this.tempFileName = Path.GetTempFileName();
                        File.Delete(tempFileName);
                        result = File.Create(tempFileName, 4096, FileOptions.DeleteOnClose);
                        if (fileMode == FileMode.CreateNew)
                        {
                            //stream.WriteByte(0);
                        }
                        else
                        {
                            stream.CopyTo(result);
                            result.Position = 0;
                        }
                    }
                }
                reader.Close();
                trans.Commit();
            }
            base.setStream(result);
        }

        void IDisposable.Dispose() => base.Dispose();

        /// <summary>
        /// Closes the underlying file stream after updating the File's content in the database if has been modified.
        /// </summary>
        public override void Close()
        {
            if (modified)
            {
                update();
            }
            base.Close();
        }

        /// <summary>
        /// Updates the file content in database with the content of the underlying stream.
        /// </summary>
        private void update()
        {
            //TODO: Cleanup embedded T-SQL
            var commandText = $"Select file_stream.PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() from {fileTableName} where stream_id={DbConstants.StreamIdParameterName}";

            //TODO: Isolate dabase access
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var trans = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                var command = new SqlCommand(commandText, connection);
                command.Transaction = trans;
                command.Parameters.AddWithValue(DbConstants.StreamIdParameterName, stream_Id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string path = reader.GetString(0);
                    byte[] transactionContext = reader.GetSqlBytes(1).Buffer;
                    var buffer = new byte[4096];
                    using (var stream = new System.Data.SqlTypes.SqlFileStream(path, transactionContext, FileAccess.ReadWrite, FileOptions.SequentialScan, allocationSize: 0))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        this.Seek(0, SeekOrigin.Begin);
                        var read = this.Read(buffer, 0, buffer.Length);
                        while (read > 0)
                        {
                            stream.Write(buffer, 0, read);
                            read = this.Read(buffer, 0, buffer.Length);
                        }
                        //this.CopyTo(stream); <-- doesn't seem to work
                        stream.Flush();
                        stream.Close();
                    }
                }
                reader.Close();
                trans.Commit();
            }
        }
    }
}
