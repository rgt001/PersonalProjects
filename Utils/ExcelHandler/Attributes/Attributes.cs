using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.ExcelHandler.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class DelimitedMember : Attribute
    {
        public readonly int Index;
        public readonly int RangeIndex;

        public DelimitedMember(int index)
        {
            Index = index;
        }

        public DelimitedMember(int index, int rangeIndex)
        {
            Index = index;
            RangeIndex = rangeIndex;
        }
    }
}
