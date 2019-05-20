using System;
using System.Linq;

namespace Sql.IO
{
    /// <summary>
    /// Provides utilities from create new Locator Path's used in Native SQL Heirachy Ids.
    /// </summary>
    public struct SqlLocatorId
    {
        /// <summary>
        /// The string representation of the Locator Id for this node, formatted as [LowValue].[MidValue].[LongValue]
        /// </summary>
        private string m_value { get; set; }

        /// <summary>
        /// Initialize a new Sql Locator from a string in the format of [LowValue].[MidValue].[LongValue]
        /// </summary>
        /// <param name="locatorId">A Locator Id in the format of [LowValue].[MidValue].[LongValue]</param>
        public SqlLocatorId(string locatorId) : this()
        {
            //TODO: Internally, only using this class to load as single locator Id. Should probably valid the the string only contains a single id
            // and isn't a full path representation of a heirachyId containing multiple locator ids
            m_value = locatorId.Trim(Constants.BackslashChars);
        }

        /// <summary>
        /// Creates a <see cref="SqlLocatorId"/> from the specified <see cref="long"/> values.
        /// </summary>
        /// <param name="first">The first value of the <see cref="SqlLocatorId"/></param>
        /// <param name="middle">the middle value of the <see cref="SqlLocatorId"/></param>
        /// <param name="last">the last value of the <see cref="SqlLocatorId"/></param>
        public SqlLocatorId(long first, long middle, long last) : this()
        {
            m_value = string.Format(Constants.SqlLocatorIdFormat, first, middle, last);
        }


        /// <summary>
        /// An array of longs repesented in the <see cref="SqlLocatorId"/>
        /// </summary>
        public long[] LongValues => m_value.Split(Constants.PeriodChar).Select(x => long.Parse(x)).ToArray();

        /// <summary>
        /// The string representation of the <see cref="SqlLocatorId"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => m_value;

        /// <summary>
        /// Converts the <see cref="SqlLocatorId"/> to a <see cref="Guid"/>
        /// </summary>
        /// <returns></returns>
        public Guid ToGuid()
        {
            var longs = m_value.Split(Constants.PeriodChar).Select(x => long.Parse(x)).ToArray();
            var guidBytes = (byte[])guidBuffer.Clone();
            longToGuidByteSlice(guidBytes, longs[0], 0, 6);
            longToGuidByteSlice(guidBytes, longs[1], 6, 6);
            longToGuidByteSlice(guidBytes, longs[2], 12, 4);
            return new Guid(guidBytes);
        }

        /// <summary>
        /// Creates a new unique <see cref="SqlLocatorId"/> using <see cref="Guid.NewGuid()"/>
        /// </summary>
        /// <returns></returns>
        public static SqlLocatorId NewId() => Parse(Guid.NewGuid());

        //TODO: support working with parent/child locators
        //public static SqlLocator NewLocator(string parentPath) => new SqlLocator($"{parentPath}{NewLocator().m_value}/");

        //TODO: support working with parent/child locators
        //public static SqlLocator NewLocator(SqlLocator parent) => NewLocator(parent.m_value);

        /// <summary>
        /// A static function to parse <see cref="Guid"/> <see cref="byte"/>s from a Little Edian longs.
        /// </summary>
        /// <param name="guidBytes">The target byte array to copy the <see cref="long"/> <see cref="byte"/>s to</param>
        /// <param name="longValue">The input <see cref="long"/> to parse Little Edian <see cref="byte"/>s from</param>
        /// <param name="start">The start index of the <paramref name="guidBytes"/> to copy to</param>
        /// <param name="count">The number of bytes to copy</param>
        private static void longToGuidByteSlice(byte[] guidBytes, long longValue, int start, int count)
        {
            byte[] longBytes = BitConverter.GetBytes(longValue);
            int k = count - 1;
            int j = start;
            for (var i = 0; i < count; i++, j++, k--)
            {
                guidBytes[j] = longBytes[k];
            }
        }

        static byte[] guidBuffer = new byte[16];

        /// <summary>
        /// Parses the <see cref="Guid"/>s from a path containing <see cref="SqlLocatorId"/>s.
        /// </summary>
        /// <param name="locatorPath"></param>
        /// <returns></returns>
        public static Guid[] ParseGuids(string locatorPath)
            => locatorPath.Split(Constants.BackslashChars, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new SqlLocatorId(x).ToGuid()).ToArray();

        /// <summary>
        /// A static empty buffer for parsing <see cref="long"/>s from a <see cref="byte"/> array used to minimize allocations.
        /// </summary>
        static byte[] longBuffer = new byte[8];

        /// <summary>
        /// A static function for parsing a long from a slice of a Big Edian <see cref="byte"/> array.
        /// </summary>
        static Func<byte[], int, int, long> longFromGuidBytes = (guidBytes, start, count) =>
        {
            var buffer = (byte[])longBuffer.Clone();
            int k = 0;
            for (var i = (start + count) - 1; k < count; i--, k++) buffer[k] = guidBytes[i];
            return BitConverter.ToInt64(buffer, 0);
        };

        /// <summary>
        /// Parses a <see cref="SqlLocatorId"/> from the bytes of a <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static SqlLocatorId Parse(Guid guid)
        {
            var bytes = guid.ToByteArray();
            var first = longFromGuidBytes(bytes, 0, 6);
            var middle = longFromGuidBytes(bytes, 6, 6);
            var last = longFromGuidBytes(bytes, 12, 4);
            return new SqlLocatorId(first, middle, last);
        }
    }
}
