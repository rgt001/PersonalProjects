using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLApi
{
    public class Program
    {
        static void Main(string[] args)
        {
            DuoCheck duoCheck = new DuoCheck();
            Request.ApiKey = File.ReadAllText("Key.txt");

            do
            {
                while (!duoCheck.Check("PlaceHolder"))//Your summoner name
                { 
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Error, retrying..."); 
                };

                Console.ReadLine();
            } while (true);
        }
    }
}
