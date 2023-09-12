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
    public class CategoryViewModel : ICopyFrom<CategoryModel>
    {
        [PossibleNames("ID")]
        public int ID { get; set; }

        [PossibleNames("Name")]
        public string CategName { get; set; }

        public void CopyFrom(CategoryModel source)
        {
            this.ID = source.ID;
            this.CategName = source.CategName;
        }
    }
}
