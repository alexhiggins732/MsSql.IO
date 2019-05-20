using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IOTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            var uncInfo = UncInfo.Default();
            var tests = new Sql.IO.Tests.SqlPathTests(uncInfo);
            tests.GetFileSystemInfoTest();
        
        }
      
    }
}
