using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Conversions
{
    public static class Bool
    {
        public static int ToInt(this bool source)
        {
            if (source)
                return 1;

            return 0;
        }

        public static string BToString(this bool source)
        {
            if (source)
                return "1";

            return "0";
        }
    }
}
