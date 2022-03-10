using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LiteDB;

namespace DB5
{
    class Score
    {
        public string Namn { get; set; }
        public int Gissningar { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Be om användarens namn och sätt gissningar till 0
            Console.WriteLine("Välkommen! Vad heter du?");
            string namn = Console.ReadLine();
            int gissningar = 0;

            // Slumpa fram rätt svar
            Random r = new Random();
            int korrekt = r.Next(1, 11);

            while (true)
            {
                // Be om gissning
                gissningar++;
                Console.WriteLine("Gissa: ");
                int gissning;
                while (int.TryParse(Console.ReadLine(), out gissning) == false)
                {
                    // Acceptera bara siffror
                    Console.WriteLine("Ogiltigt! Försök igen.");
                }

                // Berätta information
                if (gissning > korrekt)
                {
                    Console.WriteLine("Du gissade för högt.");
                }
                else if (gissning < korrekt)
                {
                    Console.WriteLine("Du gissade för lågt.");
                }
                else
                {
                    // Om det blev rätt - avbryt loopen
                    break;
                }
            }

            // Vi är klara!
            // säg det
            Console.WriteLine("Helt rätt! Du behövde " + gissningar + " gissningar!");

            // och spara detta i databasen
            Score nytt_resultat = new Score { Namn = namn, Gissningar = gissningar };
            using (var db = new LiteDatabase("highscore.db"))
            {
                var results = db.GetCollection<Score>("results");

                results.Insert(nytt_resultat);
            }

            // Skriv ut high score-listan
            Console.WriteLine(" === RESULTAT === ");
            using (var db = new LiteDatabase("highscore.db"))
            {
                var results = db.GetCollection<Score>("results");

                // Skriv ut antalet resultat totalt
                var antal = results.FindAll().Count();
                Console.WriteLine("Antal i high score listan: " + antal);

                // Skriv ut alla resultat
                Console.WriteLine("--- (alla)");
                var alla_resultat = results.FindAll().ToList();
                for (int i = 0; i < alla_resultat.Count; i++)
                {
                    Console.WriteLine(alla_resultat[i].Namn + ": " + alla_resultat[i].Gissningar);
                }

                // Skriv ut alla resultat för mig
                Console.WriteLine("--- (mina)");
                // TODO kan vi göra detta bättre? Ja, med ett index!
                // Se funktionen EnsureIndex på länken i uppgift 7
                var mina_resultat = results.Find(d => d.Namn == namn).ToList();
                for (int i = 0; i < mina_resultat.Count; i++)
                {
                    Console.WriteLine(mina_resultat[i].Namn + ": " + mina_resultat[i].Gissningar);
                }

                // Hur många resultat har respektive person?
                Console.WriteLine("---");
                Dictionary<string, int> occurences = new Dictionary<string, int>();
                for (int i = 0; i < alla_resultat.Count; i++)
                {
                    string n = alla_resultat[i].Namn;
                    if (occurences.ContainsKey(n))
                    {
                        // Detta namn är redan med i tabellen
                        occurences[n] += 1;
                    }
                    else
                    {
                        // Vi har inte sett det här namnet förut
                        occurences[n] = 1;
                    }
                }
                foreach (var o in occurences)
                {
                    Console.WriteLine(o.Key + ": " + o.Value);
                }

                // Mest flitiga spelaren
                Console.WriteLine("---");
                string bestName = "";
                int bestValue = 0;
                foreach (var o in occurences)
                {
                    if (o.Value > bestValue)
                    {
                        bestValue = o.Value;
                        bestName = o.Key;
                    }
                }
                Console.WriteLine(bestName + ": " + bestValue);

                // Skriv ut top 3 resultat
                Console.WriteLine("--- (top 3)");
                alla_resultat.Sort((a, b) => a.Gissningar.CompareTo(b.Gissningar));
                for (int i = 0; i < Math.Min(3, alla_resultat.Count); i++)
                {
                    Console.WriteLine(alla_resultat[i].Namn + ": " + alla_resultat[i].Gissningar);
                }
            }
        }
    }
}

