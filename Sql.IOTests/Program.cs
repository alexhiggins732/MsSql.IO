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
            var tests = new Sql.IO.Tests.SqlPathTests();
            tests.GetFileSystemInfoTest();

        
        }
    }
}
