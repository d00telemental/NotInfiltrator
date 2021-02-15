using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotInfiltrator.Serialization.Data
{
    public abstract class Data
    {
        public int Id { get; protected set; } = 0;
        public StructBin StructBin { get; protected set; } = null;

        protected Data(int id, StructBin sbin)
        {
            Id = id;
            StructBin = sbin;
        }
    }
}
