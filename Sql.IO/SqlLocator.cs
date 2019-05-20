using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sql.IO
{
    /// <summary>
    /// Static utilities for working with <see cref="SqlLocatorId"/>s
    /// </summary>
    public static class SqlLocator
    {
        /// <summary>
        /// An extension to easily convert a <see cref="Guid"/> to a <see cref="SqlLocatorId"/>
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static SqlLocatorId ToSqlLocator(this Guid guid) => SqlLocatorId.Parse(guid);
    }
}
