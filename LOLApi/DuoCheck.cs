using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LOLApi.Classes;

namespace LOLApi
{
    public class DuoCheck
    {
        public bool Check(string SummonerName)
        {
            try
            {
                Summoner summoner = Request.GetSummonerByName(SummonerName);
                Spectate currentMatch = Request.GetSummonerCurrentMatch(summoner.id);
                Participant requestedSummoner = currentMatch.participants.Where(p => p.summonerName == SummonerName).First();
                List<Participant> enemies = currentMatch.participants.Where(p => p.teamId != requestedSummoner.teamId).ToList();
                List<Participant> allies = currentMatch.participants.Where(p => p.teamId == requestedSummoner.teamId).ToList();
                Dictionary<Tuple<string, string>, int> matchQuantityEnemies = GetMatchesTogether(enemies);
                Dictionary<Tuple<string, string>, int> matchQuantityAllies = GetMatchesTogether(allies);

                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var item in matchQuantityEnemies.Where(p => p.Value > 1).OrderByDescending(p => p.Value))
                {
                    Console.WriteLine($"{item.Key.Item1}|{item.Key.Item2} with {item.Value} matches together");
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var item in matchQuantityAllies.Where(p => p.Value > 1).OrderByDescending(p => p.Value))
                {
                    Console.WriteLine($"{item.Key.Item1}|{item.Key.Item2} with {item.Value} matches together");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Dictionary<Tuple<string, string>, int> GetMatchesTogether(List<Participant> participants)
        {
            Dictionary<Summoner, List<long>> summoners = new Dictionary<Summoner, List<long>>();
            foreach (var participant in participants)
            {
                Summoner summoner = Request.GetSummonerByName(participant.summonerName);
                List<long> matches = Request.GetSummonerMatches(summoner.accountId).matches.Select(p => p.gameId).ToList();

                summoners.Add(summoner, matches);
            }

            Dictionary<Tuple<string, string>, int> matchQuantity = new Dictionary<Tuple<string, string>, int>();
            foreach (KeyValuePair<Summoner, List<long>> summoner in summoners)
            {
                foreach (KeyValuePair<Summoner, List<long>> anotherParticipant in summoners.Where(p => p.Key.id != summoner.Key.id))
                {
                    int temp = summoner.Value.Except(anotherParticipant.Value).Count();
                    matchQuantity.Add(new Tuple<string, string>(summoner.Key.name, anotherParticipant.Key.name), 100 - temp);
                }
            }

            return matchQuantity;
        }
    }
}
