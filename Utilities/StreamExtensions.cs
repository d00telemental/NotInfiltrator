﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NotInfiltrator.Utilities
{
    public static class StreamExtensions
    {
        public static byte[] ReadBytes(this Stream stream, int num)
        {
            var bytes = new byte[num];
            var read = stream.Read(bytes, 0, num);
            if (read != num)
            {
                throw new Exception();
            }
            return bytes;
        }

        public static string ReadAscFixed(this Stream stream, int len)
        {
            return Encoding.ASCII.GetString(stream.ReadBytes(len));
        }

        public static Int16 ReadSigned16Little(this Stream stream)
        {
            var bytes = stream.ReadBytes(2);

            Int64 i = 0;

            i |= bytes[1] << 8;
            i |= bytes[0] << 0;

            return (Int16)(i & 0xFFFF);
        }

        public static Int32 ReadSigned32Little(this Stream stream)
        {
            var bytes = stream.ReadBytes(4);

            Int64 i = 0;

            i |= bytes[3] << 24;
            i |= bytes[2] << 16;
            i |= bytes[1] << 8;
            i |= bytes[0] << 0;

            return (Int32)(i & 0xFFFFFFFF);
        }

        public static Int64 ReadSigned64Little(this Stream stream)
        {
            var bytes = stream.ReadBytes(8);

            Int64 i = 0;

            i |= bytes[7] << 56;
            i |= bytes[6] << 48;
            i |= bytes[5] << 40;
            i |= bytes[4] << 32;
            i |= bytes[3] << 24;
            i |= bytes[2] << 16;
            i |= bytes[1] << 8;
            i |= bytes[0] << 0;

            return (Int64)(i);
        }

        public static UInt16 ReadUnsigned16Little(this Stream stream)
        {
            var bytes = stream.ReadBytes(2);

            UInt64 i = 0;

            i |= (ulong)(short)(bytes[1] << 8);
            i |= (ulong)(short)(bytes[0] << 0);

            return (UInt16)(i & 0xFFFF);
        }

        public static UInt32 ReadUnsigned32Little(this Stream stream)
        {
            var bytes = stream.ReadBytes(4);

            UInt64 i = 0;

            i |= (ulong)(short)(bytes[3] << 24);
            i |= (ulong)(short)(bytes[2] << 16);
            i |= (ulong)(short)(bytes[1] << 8);
            i |= (ulong)(short)(bytes[0] << 0);

            return (UInt32)(i & 0xFFFFFFFF);
        }

        public static UInt64 ReadUnsigned64Little(this Stream stream)
        {
            var bytes = stream.ReadBytes(8);

            UInt64 i = 0;

            i |= (ulong)(short)(bytes[7] << 56);
            i |= (ulong)(short)(bytes[6] << 48);
            i |= (ulong)(short)(bytes[5] << 40);
            i |= (ulong)(short)(bytes[4] << 32);
            i |= (ulong)(short)(bytes[3] << 24);
            i |= (ulong)(short)(bytes[2] << 16);
            i |= (ulong)(short)(bytes[1] << 8);
            i |= (ulong)(short)(bytes[0] << 0);

            return (UInt64)(i);
        }
    }
}