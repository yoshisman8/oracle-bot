using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Oracle.Services;
using LiteDB;
using Discord;

namespace Oracle.Data
{
    public class Encounter
    {
        [BsonId]
        public ulong Channel { get; set; }
        public List<Participant> Participants { get; set; }
        public Participant Current { get; set; }
        public int Round { get; set; } = 1;
        public bool Active { get; set; }

        /// <summary>
        /// Selects and returns the next person in the inititaive order.
        /// </summary>
        /// <returns>The current Participant.</returns>
        public Participant Next()
        {
            GetList();
            int Index = Participants.IndexOf(Current);
            if (Participants.Count + 1 >= Participants.Count)
            {
                Current = Participants[0];
                Round++;
                return Current;
            }
            else
            {
                Current = Participants[Index + 1];
                return Current;
            }
        }
        public Participant Previous()
        {
            GetList();
            int Index = Participants.IndexOf(Current);
            if (Participants.Count - 1 < 0)
            {
                Current = Participants.Last();
                Round++;
                return Current;
            }
            else
            {
                Current = Participants[Index - 1];
                return Current;
            }
        }
        /// <summary>
        /// Sorts the initiative and returns the embed with the current person in initiative.
        /// </summary>
        /// <returns>The encounter Embed.</returns>
        public Embed Start()
        {
            GetList();
            Round = 1;
            Current = Participants[0];
            Active = true;
            return GetEncounter();
        }
        /// <summary>
        /// Clears the current encounter.
        /// </summary>
        public void End()
        {
            Current = null;
            Participants = new List<Participant>();
            Round = 0;
            Active = false;
        }
        public string Add(string Name, int Initiative, ulong Player, Type type = Type.NPC, int ID = -1)
        {
            
            if(Participants.Exists(x=> x.Name.ToLower() == Name.ToLower()))
            {
                var P = Participants.Find(x => x.Name.ToLower() == Name.ToLower());
                var I = Participants.IndexOf(P);
                if(Current == P)
                {
                    P.Initiative = Initiative;
                    Participants[I] = P;
                    Current = P;
                }
                else
                {
                    P.Initiative = Initiative;
                    Participants[I] = P;
  
                }
                GetList();
                return "Updated **" + P.Name + "**'s initiative to **" + P.Initiative + "**.";
            }
            else
            {
                Participant P = new Participant()
                {
                    Name = Name,
                    Initiative = Initiative,
                    Player = Player,
                    Type = type,
                    ID = ID
                };
                Participants.Add(P);
                GetList();
                return "Added " + P.Type + " **" + P.Name + "** to the curring encounter with an initiative of **" + P.Initiative + "**.";
            }
        }
        /// <summary>
        /// Removes a participant from an encounter.
        /// </summary>
        /// <param name="Name">Name of the participant</param>
        /// <returns>Success message or null if no participant was found.</returns>
        public string Remove(string Name)
        {
            var P = Participants.Find(x => x.Name.ToLower().StartsWith(Name.ToLower()));
            if (P == null) return null;
            else {
                if (Current == P) Previous();
                Participants.Remove(P);
                return "Removed **" + P.Name + "** from the encounter.";
            }
        }
        /// <summary>
        /// Sorts and Returns the list of participants.
        /// </summary>
        /// <returns>The Participant list.</returns>
        public List<Participant> GetList()
        {
            Participants = Participants.OrderByDescending(x => x.Initiative).ToList();
            return Participants;
        }
        /// <summary>
        /// Builds and returns the encounter's Embed/
        /// </summary>
        /// <returns>The encounter Embed</returns>
        public Embed GetEncounter()
        {
            var part = GetList();

            StringBuilder sb = new StringBuilder();

            foreach(var x in part)
            {
                if(Current == x)
                {
                    sb.AppendLine("**[" + x.Initiative + "] **" + x.Name + ".**");
                }
                else
                {
                    sb.AppendLine("[" + x.Initiative + "] " + x.Name + ".");
                }
            }
            EmbedBuilder EB = new EmbedBuilder()
                .WithTitle("Encounter Summary")
                .WithDescription("Round: "+Round)
                .AddField("Initiative", sb.ToString(),false)
                .WithCurrentTimestamp();

            return EB.Build();
        }
    }
    public class Participant
    {
        public string Name { get; set; }
        public int ID { get; set; } = -1;
        public Type Type { get; set; } = Type.NPC;
        public int Initiative { get; set; }
        public ulong Player { get; set; }
    }
    public enum Type { Human, Guardian, NPC}
}
