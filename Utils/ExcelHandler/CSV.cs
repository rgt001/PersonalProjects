using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.ExcelHandler.Attributes;

namespace Utils.ExcelHandler
{
    public class CSV : OpenBase
    {
        public override IEnumerable<IEnumerable<string>> InternalOpen(string path, bool hasHeader = true)
        {
            using (StreamReader reader = new StreamReader(path, Encoding.GetEncoding(1252), true))
            {
                string line;
                if (hasHeader)
                    reader.ReadLine();

                while ((line = reader.ReadLine()) != null)
                {
                    //yield return line.Split(new string[] { CultureInfo.InvariantCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    yield return line.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }
    }
}
