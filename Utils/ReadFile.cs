using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class ReadFile
    {
        public static string TextRead(string path)
        {
            try
            {
                string[] lines = File.ReadAllLines(path, Encoding.GetEncoding("iso-8859-1"));

                StringBuilder stringBuilder = new StringBuilder();
                for (int cont = 1; cont < lines.Length; cont++)
                {
                    stringBuilder.AppendLine(lines[cont]);
                }

                return stringBuilder.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
