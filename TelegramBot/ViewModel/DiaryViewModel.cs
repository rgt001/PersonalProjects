using SqlServer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Models;
using Utils.LorealPersistorHelpers;
using Utils.PSLikeOutput;

namespace TelegramBot.ViewModel
{
    public class DiaryViewModel : ICopyFrom<DiaryModel>
    {
        [PossibleNames("ID")]
        public int ID { get; set; }

        [PossibleNames("Categ", "CategoryID", "CID")]
        public int CategoryID { get; set; }

        [PossibleNames("Event", "EventInformation", "EI")]
        public string EventInformation { get; set; }

        [PossibleNames("Date", "RecordDate", "D")]
        public DateTime RecordDate { get; set; }

        public void CopyFrom(DiaryModel source)
        {
            if (source == null)
                source = new DiaryModel();

            this.ID = source.ID;
            this.CategoryID = source.CategoryID;
            this.EventInformation = source.EventInformation;
            this.RecordDate = source.RecordDate;
        }


    }
}
