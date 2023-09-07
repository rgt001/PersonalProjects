using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class EanHelper
    {
        public static string NormalizeEan(this string ean)
        {
            ean = ean.Replace(" ", "");
            for (int cont = ean.Length; cont < 13; cont++)
                ean = "0" + ean;
            return ean;
        }
    }
}
