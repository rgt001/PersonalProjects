using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Conversions;
using Utils.ExcelHandler.Attributes;
using System.Reflection;
using System.ComponentModel;

namespace Utils.ExcelHandler
{
    public class XLSX : OpenBase
    {
        public void Create()
        {

        }

        public override IEnumerable<IEnumerable<string>> InternalOpen(string path, bool hasReader = true)
        {
            using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(path)))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelWorksheet myWorksheet = xlPackage.Workbook.Worksheets.First();
                int totalRows = myWorksheet.Dimension.End.Row;
                int totalColumns = myWorksheet.Dimension.End.Column;

                for (int rowNum = hasReader.ToInt() + 1; rowNum <= totalRows; rowNum++)
                {
                    IEnumerable<string> rowItems = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());

                    yield return rowItems;
                }
            }
        }
    }
}
