using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelToTelegramBot
{
    public class TelegramMessage
    {
        public bool Ok { get; set; }
        public Result[] Result { get; set; }
    }

    public class Result
    {
        public int Update_id { get; set; }
        public MessageBody Message { get; set; }
    }

    public class MessageBody
    {
        public int Message_id { get; set; }
        public From From { get; set; }
        public Chat Chat { get; set; }
        public long Date { get; set; }
        public DateTime DateAsDateTime { get { return DateTimeOffset.FromUnixTimeSeconds(Date).LocalDateTime; } }
        public string Text { get; set; }
    }

    public class From
    {
        public int Id { get; set; }
        public bool Is_bot { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Username { get; set; }
        public string Language_code { get; set; }
    }

    public class Chat
    {
        public int Id { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Username { get; set; }
        public string Type { get; set; }
    }
}
