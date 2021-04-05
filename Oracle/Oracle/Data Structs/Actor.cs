using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Interactivity;
using LiteDB;
using Oracle.Services;

namespace Oracle.Data
{
    public class Actor
    {
        /// <summary>
        /// ID of the actor
        /// </summary>
        [BsonId]
        public int ID { get; set; }
        /// <summary>
        /// ID of the owner
        /// </summary>
        public ulong Owner { get; set; }
        /// <summary>
        /// Name of the Branded
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Name of the guardian
        /// </summary>
        public string Name2 { get; set; }
        /// <summary>
        /// Character Avatar
        /// </summary>
        public string Avatar { get; set; } = "";
        /// <summary>
        /// Guardian Avatar
        /// </summary>
        public string Avatar2 { get; set; } = "";
        /// <summary>
        /// List of Attributes of the Branded
        /// </summary>
        public Dictionary<string, int> Ranks { get; set; } = new Dictionary<string, int>()
        {
            {"intelligence",1 },
            {"wits",1 },
            {"resolve",1 },
            {"strength",1 },
            {"dexterity",1 },
            {"stamina",1 },
            {"presence",1 },
            {"manipulation",1 },
            {"composure",1 },
            {"academics",0 },
            {"computers",0 },
            {"crafts",0 },
            {"investigation",0 },
            {"medicine",0 },
            {"occult",0 },
            {"politics",0 },
            {"science",0 },
            {"athletics",0 },
            {"brawl",0 },
            {"drive",0 },
            {"firearms",0 },
            {"larceny",0 },
            {"stealth",0 },
            {"survival",0 },
            {"weaponry",0 },
            {"animal-ken",0 },
            {"empathy",0 },
            {"expression",0 },
            {"intimidation",0 },
            {"persuasion",0 },
            {"socialize",0 },
            {"streetwise",0 },
            {"subterfuge",0 },
            {"death",0 },
            {"fate",0 },
            {"forces",0 },
            {"life",0 },
            {"matter",0 },
            {"mind",0 },
            {"prime",0 },
            {"space",0 },
            {"spirit",0 },
            {"time",0 },
            {"wisdom",7 },
            {"gnosis",1 },
            {"size",5 },
            {"armor",0 },
            {"willpower",0 },
            {"health",0 },
            {"ballistic-armor",0 }
        };
        /// <summary>
        /// List of Attributes of the Guardian
        /// </summary>
        public Dictionary<string, int> Ranks2 { get; set; } = new Dictionary<string, int>()
        {
            {"intelligence",1 },
            {"wits",1 },
            {"resolve",1 },
            {"strength",1 },
            {"dexterity",1 },
            {"stamina",1 },
            {"presence",1 },
            {"manipulation",1 },
            {"composure",1 },
            {"academics",0 },
            {"computers",0 },
            {"crafts",0 },
            {"investigation",0 },
            {"medicine",0 },
            {"occult",0 },
            {"politics",0 },
            {"science",0 },
            {"athletics",0 },
            {"brawl",0 },
            {"drive",0 },
            {"firearms",0 },
            {"larceny",0 },
            {"stealth",0 },
            {"survival",0 },
            {"weaponry",0 },
            {"animal-ken",0 },
            {"empathy",0 },
            {"expression",0 },
            {"intimidation",0 },
            {"persuasion",0 },
            {"socialize",0 },
            {"streetwise",0 },
            {"subterfuge",0 },
            {"size",5 },
            {"armor",0 },
            {"ballistic-armor",0 }
        };
        /// <summary>
        /// Stores the amount of damage taken
        /// </summary>
        public List<int> DamageTrack { get; set; } = new List<int>();

        [BsonIgnore]
        public int Health { get { return Math.Min(Ranks["size"], Ranks2["size"]) + Math.Max(Ranks2["stamina"], Ranks["stamina"]) + Ranks["health"];} }

        [BsonIgnore]
        public int MaxWillpower { get { return Math.Max(Ranks["resolve"], Ranks2["resolve"]) + Math.Max(Ranks["composure"], Ranks2["composure"]) + Ranks["willpower"]; } }
        public int Penalty { get
            {
                if (DamageTrack.Count == Health) return -3;
                else if (DamageTrack.Count == Health - 1) return -2;
                else if (DamageTrack.Count == Health - 2) return -1;
                else return 0;
            } }
        /// <summary>
        /// Current Ether
        /// </summary>
        public int Ether { get; set; } = 0;
        /// <summary>
        /// Current Willpower
        /// </summary>
        public int Willpower { get; set; } = 0;

        /// <summary>
        /// List of all items 
        /// </summary>
        public List<Item> Inventory { get; set; } = new List<Item>();
        /// <summary>
        /// List of the Branded's Merits
        /// </summary>
        public List<Merit> Merits { get; set; } = new List<Merit>();
        /// <summary>
        /// List of the Guardian's Merits
        /// </summary>
        public List<Merit> Merits2 { get; set; } = new List<Merit>();
        /// <summary>
        /// List of all Rotes
        /// </summary>
        public List<Rote> Rotes { get; set; } = new List<Rote>();

        public List<string> Specialties { get; set; } = new List<string>();

        public List<string> Specialties2 { get; set; } = new List<string>();

        public string[] StrongArcana { get; set; } = new string[0];
        public string[] WeakArcana { get; set; } = new string[0];

        public string[] OrderSkills { get; set; } = new string[0];

        public int Beats { get; set; } = 0;
        public int Experience { get; set; } = 0;

        public Dictionary<string, string> Macros { get; set; } = new Dictionary<string, string>();
        public PageBuilder[] GetSheet()
        {
            PageBuilder[] eb = new PageBuilder[2];
            eb[0] = new PageBuilder();
            eb[0].WithTitle(Name);
            eb[0].WithThumbnailUrl(Avatar ?? "");
            eb[0].WithDescription("**Health**\n" + GetHealthBar() + "\n" +
                    "**Ether** [" + Ether + "/" + Icons.MaxEther[Ranks["gnosis"]] + "]\n" +
                    GetEtherBar() + "\n" +
                    "**Willpower** [" + Willpower + "/" + MaxWillpower + "]" + "\n" +
                    GetWillPowerBar());
            eb[0].AddField("Mental Attributes", "Intelligence:" + RenderDots(Ranks["intelligence"]) + "\n" +
                    "Wits:" + RenderDots(Ranks["wits"]) + "\n" +
                    "Resolve:" + RenderDots(Ranks["resolve"]), true);
            eb[0].AddField("Physical Attributes", "Strength:" + RenderDots(Ranks["strength"]) + "\n" +
                    "Dexterity:" + RenderDots(Ranks["dexterity"]) + "\n" +
                    "Stamina:" + RenderDots(Ranks["stamina"]), true);
            eb[0].AddField("Social Attributes", "Presence:" + RenderDots(Ranks["presence"]) + "\n" +
                    "Manipulation:" + RenderDots(Ranks["manipulation"]) + "\n" +
                    "Composure:" + RenderDots(Ranks["composure"]), true);
            eb[0].AddField("Mental Skills", "Academics: " + RenderDots(Ranks["academics"]) + "\n" +
                    "Computers: " + RenderDots(Ranks["computers"]) + "\n" +
                    "Crafts: " + RenderDots(Ranks["crafts"]) + "\n" +
                    "Investigation: " + RenderDots(Ranks["investigation"]) + "\n" +
                    "Medicine: " + RenderDots(Ranks["medicine"]) + "\n" +
                    "Occult: " + RenderDots(Ranks["occult"]) + "\n" +
                    "Politics: " + RenderDots(Ranks["politics"]) + "\n" +
                    "Science: " + RenderDots(Ranks["science"]), true);
            eb[0].AddField("Physical Skills", "Athletics: " + RenderDots(Ranks["athletics"]) + "\n" +
                    "Brawl: " + RenderDots(Ranks["brawl"]) + "\n" +
                    "Drive: " + RenderDots(Ranks["drive"]) + "\n" +
                    "Firearms: " + RenderDots(Ranks["firearms"]) + "\n" +
                    "Larceny: " + RenderDots(Ranks["larceny"]) + "\n" +
                    "Stealth: " + RenderDots(Ranks["stealth"]) + "\n" +
                    "Survival: " + RenderDots(Ranks["survival"]) + "\n" +
                    "Weaponry: " + RenderDots(Ranks["weaponry"]), true);
            eb[0].AddField("Social Skills", "Animal-Ken: " + RenderDots(Ranks["animal-ken"]) + "\n" +
                    "Empathy: " + RenderDots(Ranks["empathy"]) + "\n" +
                    "Expression: " + RenderDots(Ranks["expression"]) + "\n" +
                    "Intimidation: " + RenderDots(Ranks["intimidation"]) + "\n" +
                    "Persuasion: " + RenderDots(Ranks["persuasion"]) + "\n" +
                    "Socialize: " + RenderDots(Ranks["socialize"]) + "\n" +
                    "Streetwise: " + RenderDots(Ranks["streetwise"]) + "\n" +
                    "Subterfuge: " + RenderDots(Ranks["subterfuge"]), true);
            eb[0].AddField("Extra", "Specialties: "+string.Join("; ", Specialties)+".\nOrder Skills: " + string.Join("; ", OrderSkills) + ".");
            eb[0].AddField("Arcana I", "Death: " + RenderDots(Ranks["death"]) + "\n" +
                    "Fate: " + RenderDots(Ranks["fate"]) + "\n" +
                    "Forces: " + RenderDots(Ranks["forces"]) + "\n" +
                    "Life: " + RenderDots(Ranks["life"]) + "\n" +
                    "Matter: " + RenderDots(Ranks["matter"]) + "\n" +
                    "Ruling: "+string.Join(", ",StrongArcana),true);
            eb[0].AddField("Arcana II", "Mind: " + RenderDots(Ranks["mind"]) + "\n" +
                    "Prime: " + RenderDots(Ranks["prime"]) + "\n" +
                    "Space: " + RenderDots(Ranks["space"]) + "\n" +
                    "Spirit: " + RenderDots(Ranks["spirit"]) + "\n" +
                    "Time: " + RenderDots(Ranks["time"]) + "\n" +
                    "Inferior: " + string.Join(", ", WeakArcana), true);
            eb[0].AddField("Statistics", "Wisdom: " + Ranks["wisdom"] + "\n" +
                    "Gnosis: " + Ranks["gnosis"] + "\n" +
                    "Size: " + Ranks["size"] + "\n" +
                    "Defense: " + (Math.Min(Ranks["dexterity"], Ranks["composure"]) + Ranks["athletics"] + Ranks["armor"]) + "/" + (Math.Min(Ranks["dexterity"], Ranks["composure"]) + Ranks["athletics"] + Ranks["ballistic-armor"]) + "\n" +
                    "Perception: " + (Ranks["wits"] + Ranks["composure"])+ "\n"+
                    "Experience ("+new string('●', Beats)+"): "+Experience, true);

            eb[1] = new PageBuilder();
            eb[1].WithTitle(Name2);
            eb[1].WithThumbnailUrl(Avatar2 ?? "");
            eb[1].WithDescription("**Health**\n" + GetHealthBar() + "\n" +
                    "**Ether** [" + Ether + "/" + Icons.MaxEther[Ranks["gnosis"]] + "]\n" +
                    GetEtherBar() + "\n" +
                    "**Willpower*** [" + Willpower + "/" + MaxWillpower + "]"+"\n"+
                    GetWillPowerBar());
            eb[1].AddField("Mental Attributes", "Intelligence:" + RenderDots(Ranks2["intelligence"]) + "\n" +
                    "Wits:" + RenderDots(Ranks2["wits"]) + "\n" +
                    "Resolve:" + RenderDots(Ranks2["resolve"]), true);
            eb[1].AddField("Physical Attributes", "Strength:" + RenderDots(Ranks2["strength"]) + "\n" +
                    "Dexterity:" + RenderDots(Ranks2["dexterity"]) + "\n" +
                    "Stamina:" + RenderDots(Ranks2["stamina"]), true);
            eb[1].AddField("Social Attributes", "Presence: " + RenderDots(Ranks2["presence"]) + "\n" +
                    "Manipulation:" + RenderDots(Ranks2["manipulation"]) + "\n" +
                    "Composure:" + RenderDots(Ranks2["composure"]), true);
            eb[1].AddField("Mental Skills", "Academics: " + RenderDots(Ranks2["academics"]) + "\n" +
                    "Computers: " + RenderDots(Ranks2["computers"]) + "\n" +
                    "Crafts: " + RenderDots(Ranks2["crafts"]) + "\n" +
                    "Investigation: " + RenderDots(Ranks2["investigation"]) + "\n" +
                    "Medicine: " + RenderDots(Ranks2["medicine"]) + "\n" +
                    "Occult: " + RenderDots(Ranks2["occult"]) + "\n" +
                    "Politics: " + RenderDots(Ranks2["politics"]) + "\n" +
                    "Science: " + RenderDots(Ranks2["science"]), true);
            eb[1].AddField("Physical Skills", "Athletics: " + RenderDots(Ranks2["athletics"]) + "\n" +
                    "Brawl: " + RenderDots(Ranks2["brawl"]) + "\n" +
                    "Drive: " + RenderDots(Ranks2["drive"]) + "\n" +
                    "Firearms: " + RenderDots(Ranks2["firearms"]) + "\n" +
                    "Larceny: " + RenderDots(Ranks2["larceny"]) + "\n" +
                    "Stealth: " + RenderDots(Ranks2["stealth"]) + "\n" +
                    "Survival: " + RenderDots(Ranks2["survival"]) + "\n" +
                    "Weaponry: " + RenderDots(Ranks2["weaponry"]), true);
            eb[1].AddField("Social Skills", "Animal-Ken: " + RenderDots(Ranks2["animal-ken"]) + "\n" +
                    "Empathy: " + RenderDots(Ranks2["empathy"]) + "\n" +
                    "Expression: " + RenderDots(Ranks2["expression"]) + "\n" +
                    "Intimidation: " + RenderDots(Ranks2["intimidation"]) + "\n" +
                    "Persuasion: " + RenderDots(Ranks2["persuasion"]) + "\n" +
                    "Socialize: " + RenderDots(Ranks2["socialize"]) + "\n" +
                    "Streetwise: " + RenderDots(Ranks2["streetwise"]) + "\n" +
                    "Subterfuge: " + RenderDots(Ranks2["subterfuge"]), true);
            eb[1].AddField("Extra", "Specialties: " + string.Join("; ", Specialties2) + ".");
            eb[1].AddField("Arcana I", "Death\\*: " + RenderDots(Ranks["death"]) + "\n" +
                    "Fate\\*: " + RenderDots(Ranks["fate"]) + "\n" +
                    "Forces\\*: " + RenderDots(Ranks["forces"]) + "\n" +
                    "Life\\*: " + RenderDots(Ranks["life"]) + "\n" +
                    "Matter\\*: " + RenderDots(Ranks["matter"]) + "\n" +
                    "Ruling: " + string.Join(", ", StrongArcana), true);
            eb[1].AddField("Arcana II", "Mind\\*: " + RenderDots(Ranks["mind"]) + "\n" +
                    "Prime\\*: " + RenderDots(Ranks["prime"]) + "\n" +
                    "Space\\*: " + RenderDots(Ranks["space"]) + "\n" +
                    "Spirit\\*: " + RenderDots(Ranks["spirit"]) + "\n" +
                    "Time\\*: " + RenderDots(Ranks["time"]) + "\n" +
                    "Inferior: " + string.Join(", ", WeakArcana), true);
            eb[1].AddField("Statistics", "Wisdom\\*: " + Ranks["wisdom"] + "\n" +
                    "Gnosis\\*: " + Ranks["gnosis"] + "\n" +
                    "Size: " + Ranks2["size"] + "\n" +
                    "Defense: " + (Math.Min(Ranks2["dexterity"], Ranks2["composure"])+Ranks2["athletics"] + Ranks2["armor"]) + "/" + (Math.Min(Ranks2["dexterity"], Ranks2["composure"]) + Ranks2["athletics"] + Ranks2["ballistic-armor"]) + "\n" +
                    "Perception: " + (Ranks2["wits"] + Ranks2["composure"]),true);
            eb[1].WithFooter("Values with an asterisk(*) can only be changed with `!set` and not `!GSet`.");
            return eb;
        }
        public string GetHealthBar()
        {
            DamageTrack = DamageTrack.OrderByDescending(x=>x).ToList();
            var sb = new StringBuilder();

            int leftover = Health - DamageTrack.Count;
            for (int i = 0; i < DamageTrack.Count; i++)
            {
                sb.Append(Icons.Damage[DamageTrack[i]]);
            }

            for (int i = 0; i < leftover; i++)
            {
                sb.Append(Icons.BarIcons["health"]);
            }
            return sb.ToString();
        }
        public string GetEtherBar()
        {
            int max = Icons.MaxEther[Ranks["gnosis"]];
            var sb = new StringBuilder();
            if (max <= 10)
            {
                int diff = max - Ether;
                for(int i = 0; i < Ether; i++)
                {
                    sb.Append(Icons.BarIcons["ether"]);
                }
                for (int i = 0; i < diff; i++)
                {
                    sb.Append(Icons.BarIcons["empty"]);
                }
                return sb.ToString();
            }
            else
            {
                decimal percent = ((decimal)Ether / (decimal)max) * 10;
                var diff = 10 - Math.Ceiling(percent);
                for (int i = 0; i < percent; i++)
                {
                    sb.Append(Icons.BarIcons["ether"]);
                }
                for (int i = 0; i < diff; i++)
                {
                    sb.Append(Icons.BarIcons["empty"]);
                }
                return sb.ToString();
            }
        }
        public string GetWillPowerBar()
        {
            var sb = new StringBuilder();
            int diff = MaxWillpower - Willpower;

            for (int i = 0; i < Willpower; i++)
            {
                sb.Append(Icons.BarIcons["willpower"]);
            }
            for (int i = 0; i < diff; i++)
            {
                sb.Append(Icons.BarIcons["empty"]);
            }
            return sb.ToString();
        }

        public string RenderDots(int value)
        {
            if (value > 10)
            {
                return Icons.Dots[10] + "x " + value;
            }
            else
            {
                return Icons.Dots[value];
            }
        }
        /// <summary>
        /// Restores everything
        /// </summary>
        public void Refresh()
        {
            Ether = Icons.MaxEther[Ranks["gnosis"]];
            Willpower = MaxWillpower;
            DamageTrack = new List<int>();
        }
    }
    public class Merit
    {
        public string Name { get; set; }
        public int Ranks { get; set; } = 1;
        public string[] Description { get; set; }
    }
    public class Rote
    {
        public string Name { get; set; }
        public string[] Description { get; set; }
    }
    public class Item
    {
        public string Name { get; set; }
        public string[] Description { get; set; }
    }
    public class Note
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
