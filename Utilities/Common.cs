using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotInfiltrator.Utilities
{
    public class MyAssertException : NullReferenceException
    {
        public MyAssertException(string message) : base(message) { }
    }

    public static class Common
    {
        // https://stackoverflow.com/a/2022194
        public static int NextMultipleOfFour(int num)
            => (num + 3) & ~3;

        public static void Assert(bool cond, string msg)
        {
            if (!cond)
            {
                throw new MyAssertException(msg);
            }
        }

        public static void AssertEquals<T>(IEnumerable<T> a, IEnumerable<T> b, string msg)
            => Assert(a.SequenceEqual(b), msg);

        public static void AssertEquals(string a, string b, string msg)
            => Assert(a == b, msg);

        public static void AssertEquals(Int32 a, Int32 b, string msg)
            => Assert(a == b, msg);

        public static void AssertEquals(UInt32 a, UInt32 b, string msg)
            => Assert(a == b, msg);

        public static void AssertEquals(Int64 a, Int64 b, string msg)
            => Assert(a == b, msg);

        public static void AssertEquals(UInt64 a, UInt64 b, string msg)
            => Assert(a == b, msg);
    }
}
