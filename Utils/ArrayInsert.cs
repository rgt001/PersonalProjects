using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class ArrayInsert
    {
        public static void SetValuesToArray(this object source, object[] values)
        {
            int cont = 0;
            if (source is double[] vdouble)
            {
                Array.ForEach<object>(values, p => vdouble[cont++] = Convert.ToDouble(p));
            }
            else if (source is int[] vint)
            {
                Array.ForEach<object>(values, p => vint[cont++] = Convert.ToInt32(p));
            }
            else if (source is string[] vstring)
            {
                Array.ForEach<object>(values, p => vstring[cont++] = p.ToString());
            }
        }
    }
}
