using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LOLApi.Classes;
using System.Threading;
using System.IO;

namespace LOLApi
{
    public static class Request
    {
        public static string ApiKey;

        private static T SimpleAPIGet<T>(string link)
        {
            RestClient client = new RestClient(link);
            RestRequest request = new RestRequest(Method.GET);

            return JsonConvert.DeserializeObject<T>(MakeRequest(client, request).Content);
        }

        private static IRestResponse MakeRequest(RestClient client, RestRequest request)
        {
            IRestResponse response;
            request.AddHeader("X-Riot-Token", ApiKey);
            do
            {
                response = client.Execute(request);

                if ((int)response.StatusCode == 401/* || response.StatusCode == System.Net.HttpStatusCode.Forbidden*/)
                {
                    Console.WriteLine("Unauthorized access, put new api key");
                    ApiKey = Console.ReadLine();

                    File.WriteAllText("Key.txt", ApiKey);

                    response.StatusCode = (System.Net.HttpStatusCode)429;
                }
            } while ((int)response.StatusCode == 429);

            return response;
        }

        public static Summoner GetSummonerByName(string summonerName)
        {
            string link = $"https://br1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{summonerName}";
            Summoner summoner = SimpleAPIGet<Summoner>(link);

            return summoner;
        }

        public static Matches GetSummonerMatches(string accountId)
        {
            string link = $"https://br1.api.riotgames.com/lol/match/v4/matchlists/by-account/{accountId}";
            Matches matches = SimpleAPIGet<Matches>(link);

            return matches;
        }

        public static Spectate GetSummonerCurrentMatch(string id)
        {
            string link = $"https://br1.api.riotgames.com/lol/spectator/v4/active-games/by-summoner/{id}";
            Spectate currentMatch = SimpleAPIGet<Spectate>(link);

            return currentMatch;
        }
    }
}
