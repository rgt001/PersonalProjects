using SqlServer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Models
{
    [SqlTableName("TB_Diary")]
    public class DiaryModel
    {
        [IdentityColumn]
        public int ID { get; set; }

        public int CategoryID { get; set; }

        public string EventInformation { get; set; }

        public DateTime RecordDate { get; set; }
    }
}
