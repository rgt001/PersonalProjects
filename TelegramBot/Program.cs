using SqlServer;
using SqlServer.Attributes;
using SqlServer.DataBase;
using SqlServer.Persistor;
using SqlServer.UtilOperations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBot.Models;
using Utils;
using Utils.CollectionExtensions;
using Utils.ExcelHandler;
using Utils.ExcelHandler.Attributes;

namespace ExcelToTelegramBot
{
    class Program
    {
        private static BotHandler BotHandler = null;

        static void Main(string[] args)
        {
            try
            {
                BootStrap.ConnectionString = ConfigurationManager.ConnectionStrings["DataBaseConnection"].ConnectionString;

                //var test0 = LorealPersistor.JoinSelect<AggregateCategoryDiary>().ToList();
                //var test1 = LorealPersistor.Select<DiaryModel>().ToList();
                //var test2 = LorealPersistor.Select<DiaryModel>(p => p.EventInformation == "hum").ToList();
                //var test = LorealPersistor.Select<AggregateCategoryDiary>(p => p.Diary.EventInformation.Contains("h")).ToList();

                Console.WriteLine("Getting Bot Info");
                var botConfigString = ConfigurationManager.ConnectionStrings["BotConfig"].ConnectionString.Replace("'", "\"");
                BotHandler = Newtonsoft.Json.JsonConvert.DeserializeObject<BotHandler>(botConfigString);

                Console.WriteLine("Bot info has been taken");

                BotHandler.Bot = new TelegramBotClient(BotHandler.ApiKey);

                Console.WriteLine("Done");

                Initiliaze();

                Console.WriteLine("Bot has been initiliazed");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Expected commands
        ///     add;category;nome
        ///     add;diary;categnum;eventinformation
        ///     list; category
        ///     list; category;LikeStatement
        ///     list; diary;categoryId
        ///     list; diary;LikeStatement
        ///     categoryId;"Lucky"
        ///     remove;diaryId
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    Console.WriteLine("Message received");
                    var messageText = e.Message.Text.Split(';');
                    Console.WriteLine(messageText);
                    //BotHandler.Bot.SendTextMessageAsync(CHATID, e.Message.Text).Wait();

                    var operationUpper = messageText[0].ToUpper();
                    switch (operationUpper)
                    {
                        case "ADD":
                            HandleADD(messageText);
                            BotHandler.Bot.SendTextMessageAsync(e.Message.Chat.Id, "Successfully added").Wait();
                            break;
                        case "LIST":
                            StringBuilder stringBuilder;
                            var result = HandleList(messageText);
                            foreach (var item in result.Partition(30))
                            {
                                stringBuilder = new StringBuilder();
                                item.ForEach(p => stringBuilder.AppendLine(p));
                                BotHandler.Bot.SendTextMessageAsync(e.Message.Chat.Id, stringBuilder.ToString()).Wait();
                            }
                            break;
                        case "REMOVE":
                            HandleRemove(messageText);
                            break;
                        default:
                            if (int.TryParse(operationUpper, out int categoryId) && messageText[1].ToUpper() == "LUCKY")
                            {
                                var categories = LorealPersistor.Select<DiaryModel>(p => p.CategoryID == categoryId).ToArray();

                                Random random = new Random();
                                var chosen = random.Next(categories.Length);

                                BotHandler.Bot.SendTextMessageAsync(e.Message.Chat.Id, categories[chosen].ConvertClassToString()).Wait();
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                BotHandler.Bot.SendTextMessageAsync(e.Message.Chat.Id, "Invalid command").Wait();
            }
        }

        private static void HandleRemove(string[] messageText)
        {
            LorealPersistor.Delete(new DiaryModel() { ID = int.Parse(messageText[1]) });
        }

        private static void HandleADD(string[] messageText)
        {
            switch (messageText[1].ToUpper())
            {
                case "CATEGORY":
                    LorealPersistor.Insert(new CategoryModel() { CategName = messageText[2] });
                    break;
                case "DIARY":
                    LorealPersistor.Insert(new DiaryModel() { CategoryID = int.Parse(messageText[2]), EventInformation = messageText[3], RecordDate = DateTime.Now });
                    break;
                default:
                    throw new InvalidOperationException("Invalid Command");
            }
        }

        private static IEnumerable<string> HandleList(string[] messageText)
        {
            switch (messageText[1].ToUpper())
            {
                case "CATEGORY":
                    if (messageText.Length == 2)
                        return LorealPersistor.Select<CategoryModel>().ConvertClassToString();

                    return LorealPersistor.Select<CategoryModel>(p => p.CategName.Contains(messageText[2])).ConvertClassToString();
                case "DIARY":
                    if (int.TryParse(messageText[2], out int result))
                        return LorealPersistor.Select<DiaryModel>(p => p.CategoryID == result).ConvertClassToString();

                    return LorealPersistor.Select<DiaryModel>(p => p.EventInformation.Contains(messageText[2])).ConvertClassToString();
                default:
                    return Enumerable.Empty<string>();
            }
        }

        private static void Initiliaze()
        {
            if (!BotHandler.Bot.IsReceiving)
            {
                Console.WriteLine("StartReceiving");
                BotHandler.Bot.StartReceiving();
                Console.WriteLine("OnMessage");
                BotHandler.Bot.OnMessage += Bot_OnMessage;
                Console.WriteLine("OnMessageEdited");
                BotHandler.Bot.OnMessageEdited += Bot_OnMessage;
                Console.WriteLine("OnMessageGeneralError");
                BotHandler.Bot.OnReceiveGeneralError += Bot_OnReceiveGeneralError;
            }
        }

        private static void Bot_OnReceiveGeneralError(object sender, Telegram.Bot.Args.ReceiveGeneralErrorEventArgs e)
        {
            BotHandler.Bot.StopReceiving();
            Thread.Sleep(5_000);
            BotHandler.Bot.StartReceiving();
        }
    }

    public class BotHandler
    {
        public TelegramBotClient Bot;
        public string ApiKey { get; set; }
        public string BotName { get; set; }
    }
}
