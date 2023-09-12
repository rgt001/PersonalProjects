using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.PSLikeOutput
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class PossibleNamesAttribute : Attribute
    {
        public readonly ReadOnlyCollection<string> Name;

        public PossibleNamesAttribute(params string[] names)
        {
            Name = new ReadOnlyCollection<string>(names.Select(p => p.ToLower()).ToList());
        }
    }
}
