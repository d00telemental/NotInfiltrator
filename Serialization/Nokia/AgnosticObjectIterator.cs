using NotInfiltrator.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Nokia
{
    public struct AgnosticObjectInfo
    {
        public long StartOffset { get; set; }
        public byte Type { get; set; }
        public byte[] Data { get; set; }

        public override string ToString()
            => $"AgnosticObjectInfo at {StartOffset} of type {Type} with {Data.Length} byte(s)";
    }

    public class AgnosticObjectEnumerator
    {
        private Stream _inputStream { get; set; }
        private HashSet<byte> _metTypes { get; set; } = new ();

        private byte _readType()
        {
            var type = (byte)_inputStream.ReadByte();
            _metTypes.Add(type);
            return type;
        }

        public AgnosticObjectEnumerator(Stream inputStream)
        {
            _inputStream = inputStream;
        }

        public IEnumerable<AgnosticObjectInfo> Read()
        {
            while (_inputStream.Position < _inputStream.Length)
            {
                yield return new AgnosticObjectInfo()
                {
                    StartOffset = _inputStream.Position,
                    Type = _readType(),
                    Data = _inputStream.ReadBytes((int)_inputStream.ReadUnsigned32Little())
                };
            }
        }

        public IEnumerable<AgnosticObjectInfo> ReadAll()
            => Read().ToList();

        public IEnumerable<byte> AllMetTypes()
            => _metTypes.OrderBy(t => t);
    }
}
