using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLTricks
{
    // this is the best we can do as there are no extension methods on static classes
    // and it's not possible to add extension properties
    // an alternative is to use datetime2 in sql server, which maps properly to .NET
    public static class DateTimeExtensions
    {
        public static DateTime MinSQLValue()
        {
            return new DateTime(System.Data.SqlTypes.SqlDateTime.MinValue.TimeTicks);
        }

        public static DateTime MaxSQLValue()
        {
            return new DateTime(System.Data.SqlTypes.SqlDateTime.MaxValue.TimeTicks);
        }
    }
}
