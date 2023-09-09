using SqlServer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Models
{
    [SqlTableName("TB_Category")]
    public class CategoryModel
    {
        [IdentityColumn]
        public int ID { get; set; }

        public string CategName { get; set; }
    }
}
