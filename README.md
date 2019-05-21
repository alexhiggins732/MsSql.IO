# Sql.IO
A wrapper providing System.IO functionality for MsSql FileTables and MsSql FileStreams.

This libary allows you to work with MS SQL FileTable and MS SQL FileStreams objects using APIs similar to System.IO apis.

Sql.IO is being developed to serve as a foundation for a new C# Document Management System (DMS) 
which will offer document management functionality through a RestFul Web API.

I also doubles as new virtual file system
for use in conjunction with the Fubar Development Portable C# FTP server (https://github.com/FubarDevelopment/FtpServer).

Using SQL file tables, the file stream and directory heirarchy are exposed as windows file share.

Updates to the underlying SQL file objects are automatically synchrozized with the file in the share.

For details on how this works see : https://www.sqlshack.com/managing-data-in-sql-server-filetables/

```csharp
var newFile = new SqlFileInfo(Path.Combine(directoryFullName , "newDirectory", "newFile.txt"));

var newFileDirectory = newFile.Directory;
Assert.IsFalse(newFileDirectory.Exists);

newFileDirectory.Create();
Assert.IsTrue(newFile.Directory.Exists);

//Work directly with the file using built-in System.IO apis.
using (var sw = new StreamWriter(newFile.Create()))
{
	sw.WriteLine("Unit Test");
}
Assert.IsTrue(newFile.Exists);


var anotherDirectory = SqlDirectory.Create(Path.Combine(directoryFullName, "anotherDirectory"));
Assert.IsNotNull(anotherDirectory);
Assert.IsTrue(anotherDirectory.Exists);
Assert.IsTrue(anotherDirectory.FullName.StartsWith(directoryFullName));

newFile.MoveTo(Path.Combine(anotherDirectory.FullName, newFile.Name));


newFileDirectory.Delete();
Assert.IsFalse(newFileDirectory.Exists);
```
