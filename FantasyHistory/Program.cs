using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using CsQuery;
using System.Text.RegularExpressions;

namespace FantasyHistory
{
    public class Record
    {
        public string Name { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int TotalPoitnsFor { get; set; }
        public int TotalPoitnsAgainst { get; set; }
    }

    public class Roster
    {
        public string Name { get; set; }
        public int QB { get; set; }
        public int RB1 { get; set; }
        public int RB2 { get; set; }
        public int WR1 { get; set; }
        public int WR2 { get; set; }
        public int TE { get; set; }
        public int K { get; set; }
        public int DST { get; set; }
        public int Bench { get; set; }

    }

    public class Player
    {
        public string Name { get; set; }
        public string Postion { get; set; }
        public int Price { get; set; }
    }

    class Program
    {
        private static CQ Dom;

        static void Main(string[] args)
        {
            GetHistoricalRecords();

        }

        public static void GetHistoricalRecordWithPoints()
        {
            List<Record> records = new List<Record>();

            for (int y = 2009; y < 2019; y++)
            {

                var dom = CQ.CreateFromUrl(String.Format("http://games.espn.com/ffl/tools/finalstandings?leagueId=773179&seasonId={0}", y));

                Console.WriteLine(y + " Season");
                for (int i = 1; i < 13; i++)
                {
                    string rankrow = "#rankRow" + i;

                    string Name = dom[rankrow + " > td:nth-child(3)"].Text(); //#rankRow2 > td:nth-child(3)
                    string Record = dom[rankrow + " > td.sortableREC"].Text();
                    string pf = dom[rankrow + " > td.sortablePF"].Text();
                    string pa = dom[rankrow + " > td.sortablePA"].Text();

                    if (!String.IsNullOrEmpty(Name))
                    {
                        string wins = Record.Split('-')[0].Substring(Math.Max(0, Record.Split('-')[0].Length - 2));
                        string losses = Record.Split('-')[1];

                        if (records.Any(s => s.Name == Name))
                        {
                            var r = records.Where(s => s.Name == Name).FirstOrDefault();
                            r.Wins = r.Wins + int.Parse(wins);
                            r.Losses = r.Losses + int.Parse(losses);
                            r.TotalPoitnsFor = r.TotalPoitnsFor + int.Parse(pf);
                            r.TotalPoitnsAgainst = r.TotalPoitnsAgainst + int.Parse(pa);
                        }
                        else
                        {
                            records.Add(new Record { Name = Name, Wins = int.Parse(wins), Losses = int.Parse(losses), TotalPoitnsAgainst = int.Parse(pa), TotalPoitnsFor = int.Parse(pf) });
                        }
                        Console.WriteLine(String.Format("Name: {0} Wins: {1} Losses {2}", Name, wins, losses));
                    }
                }
            }


            List<string> outputList = new List<string>();

            foreach (var r in records.OrderBy(o => o.Wins))
            {
                var s = string.Format("{0},{1},{2},{3},{4}", r.Name, r.Wins, r.Losses, r.TotalPoitnsFor, r.TotalPoitnsAgainst);
                outputList.Add(s);

            }
            System.IO.File.WriteAllLines(@"C:\wwwroot\historicStats.csv", outputList);

            Console.ReadKey();
        }

        public static void GetHistoricalRecords()
        {

            List<Record> records = new List<Record>();

            for (int y = 2009; y < 2019; y++)
            {
                Console.WriteLine(y + " Season");
                for (int i = 1; i < 13; i++)
                {
                    var dom = CQ.CreateFromUrl(String.Format("http://games.espn.com/ffl/clubhouse?leagueId=773179&teamId={0}&seasonId={1}", i, y));

                    string Name = dom["#content > div > div.gamesmain.container > div > div > div:nth-child(3) > div.games-topcol.games-topcol-expand > div:nth-child(2) > div.games-univ-mod3 > ul:nth-child(3) > li"].Text();
                    string Record = dom["#content > div > div.gamesmain.container > div > div > div:nth-child(3) > div.games-topcol.games-topcol-expand > div:nth-child(2) > div.games-univ-mod4 > h4"].Text();
                    string regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";

                    if (!String.IsNullOrEmpty(Name))
                    {

                        Record = Regex.Replace(Record, @"<[^>]*>", String.Empty).Trim();
                        Record = Regex.Replace(Record, regex, "");
                        Record = Record.Replace("(", "").Replace(")", "").Trim();

                        string wins = Record.Split('-')[0].Replace("Record: ", "");
                        string losses = Record.Split('-')[1];

                        if (records.Any(s => s.Name == Name))
                        {
                            var r = records.Where(s => s.Name == Name).FirstOrDefault();
                            r.Wins = r.Wins + int.Parse(wins);
                            r.Losses = r.Losses + int.Parse(losses);
                        }
                        else
                        {
                            records.Add(new Record { Name = Name, Wins = int.Parse(wins), Losses = int.Parse(losses) });
                        }
                        Console.WriteLine(String.Format("Name: {0} Wins: {1} Losses {2}", Name, wins, losses));
                    }

                }
            }

            List<string> outputList = new List<string>();

            foreach (var r in records.OrderBy(o => o.Wins))
            {
                var s = string.Format("{0},{1},{2}", r.Name, r.Wins, r.Losses);
                outputList.Add(s);

            }
            System.IO.File.WriteAllLines(@"C:\wwwroot\SavedLists.csv", outputList);

            Console.ReadKey();

        }
    }
}
