using Discord.WebSocket;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Oracle.Data;

namespace Oracle.Services
{
	public static class Helpers
	{
		public static bool IsImageUrl(this string URL)
		{
			try
			{
				var req = (HttpWebRequest)HttpWebRequest.Create(URL);
				req.Method = "HEAD";
				using (var resp = req.GetResponse())
				{
					return resp.ContentType.ToLower(CultureInfo.InvariantCulture)
							.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
				}
			}
			catch
			{
				return false;
			}
		}
		public static bool NullorEmpty(this string _string)
		{
			if (_string == null) return true;
			if (_string == "") return true;
			else return false;
		}
		public static SocketTextChannel GetTextChannelByName(this SocketGuild Guild, string Name)
		{
			var results = Guild.TextChannels.Where(x => x.Name.ToLower() == Name.ToLower());
			if (results == null || results.Count() == 0) return null;
			else return results.FirstOrDefault();
		}
		public static IEnumerable<string> SplitByLength(this string str, int maxLength)
		{
			for (int index = 0; index < str.Length; index += maxLength)
			{
				yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
			}
		}
	}
	public class Utilities
    {
		private LiteDatabase db { get; set; }

		public Utilities(LiteDatabase _db)
        {
			db = _db;
        }

		public User GetUser(ulong UserId)
        {
			var col = db.GetCollection<User>("Users");

			if (col.Exists(x => x.ID == UserId)) return col.Include(x=>x.Active).FindOne(x => x.ID == UserId);
            else
            {
				User U = new User()
				{
					ID = UserId
                };
				col.Insert(U);
				return col.Include(x => x.Active).FindOne(x => x.ID == UserId);
			}
        }
		public void UpdateUser(User U)
        {
			var col = db.GetCollection<User>("Users");

			col.Update(U);
		}
		
		public void UpdateActor(Actor A)
        {
			var col = db.GetCollection<Actor>("Actors");
			col.Update(A);
        }
    }
	public static class Icons
    {
		public static Dictionary<int, string> d10 { get; set; } = new Dictionary<int, string>()
		{
			{10, "<:d10_10:663158741352579122>" },
			{9, "<:d10_9:663158741331476480>" },
			{8, "<:d10_8:663158741079687189>" },
			{7, "<:d10_7:663158742636036138>" },
			{6, "<:d10_6:663158741121761280>" },
			{5, "<:d10_5:663158740576632843>" },
			{4, "<:d10_4:663158740685553713>" },
			{3, "<:d10_3:663158740442415175>" },
			{2, "<:d10_2:663158740496810011>" },
			{1, "<:d10_1:663158740463255592>" }
		};
		
		public static Dictionary<int, string> Damage { get; set; } = new Dictionary<int, string>()
		{
			{1,"<:bashing:827589602805415997>" },
			{2,"<:lethal:827589602985902080>" },
			{3, "<:Aggravated:827589603023257630>" }
		};

		public static Dictionary<int, int> MaxEther { get; set; } = new Dictionary<int, int>()
		{
			{1,10 },
			{2,11 },
			{3,12 },
			{4,13 },
			{5,15 },
			{6,20 },
			{7,25 },
			{8,30 },
			{9,50 },
			{10,75 }
		};
		public static Dictionary<string, string> BarIcons { get; set; } = new Dictionary<string, string>()
		{
			{"ether","<:Ether:685854267373781013>" },
			{"empty","<:Empty:685854267512455178>" },
			{"willpower","<:WP:685854267466449107>" },
			{"health", "<:Health:827589986283028490>" }
		};
		public static Dictionary<int, string> Dots { get; set; } = new Dictionary<int, string>()
		{
			{10, "<:10:827589603035840513>" },
			{9, "<:9:827589602793357323>" },
			{8, "<:8:827589602738044948>" },
			{7, "<:7:827589602684436571>" },
			{6, "<:6:827589602461614111>" },
			{5, "<:5:827589602234990693>" },
			{4, "<:4:827589602272608256>" },
			{3, "<:3:827589602226339850>" },
			{2, "<:2:827589601782661161>" },
			{1, "<:1:827589602030125126>" },
			{0, "<:0:827648368544186379> " }
		};
		public static List<Attainment> Attainments { get; set; } = new List<Attainment>()
		{
			{ new Attainment("Counterspell","any",1,"Knowledge of one Arcanum imparts the understanding of how to unravel it. To cast a spell, the mage forms an Imago; to counter a spell, the mage simply needs to disrupt one. The Counterspell Attainment is actually 10 different Attainments, one for each Arcanum. By learning even the most basic principles of an Arcanum, a mage understands how to counter a spell. By the time a spell takes effect and a mage feels it in Peripheral Mage Sight, though, it’s too late to counter; to use this Attainment, the countering mage must see her rival casting in Active Mage Sight.", "Counterspell is a Clash of Wills (see p. 117), pitting the acting mage’s Gnosis + Arcanum against the countering mage’s Gnosis + Arcanum. A mage can attempt to counter any spell that uses the Arcanum, even if it uses other Arcana as well, but always counters the highest Arcanum of a target spell. The comparative ratings of the two mages’ Arcana are irrelevant; an Initiate can, in theory, counter the spell of a Master. Countering the spell of a mage with a higher rating in the target Arcanum, however, requires that the player spend a point of Mana. Counterspell requires an instant action. If the mage is employing Active Mage Sight (see p. 90), she can attempt to counter a spell of the appropriate Arcanum in combat, regardless of her position in the Initiative order, provided she has not used her action yet.") },
			{ new Attainment("Eyes of the Dead","death",2,"The mage can see ghosts and souls in Twilight when using Active Mage Sight with Death. Her Peripheral Mage Sight reacts to even the passive presence of ghosts.","The mage detects ghosts and deathly Twilight phenomena with her Periphery, and can automatically see souls and ghosts in Twilight with her Death Sight. If a ghost is using a power to hide, it provokes a Clash of Wills. With the expenditure of one point of Mana, the mage can interact with ghosts for a scene. She can speak with them, touch them, and even strike them. However, this renders her vulnerable to their attentions, as well.") },
			{ new Attainment("Conditional Duration","fate", 2, "The mage can, as well as assigning Duration with a spell factor, create a condition under which the spell ceases to function. Doing so can increase the Duration of a spell, although the mage must still spend Mana and a Reach if Duration becomes indefinite. The more improbable the condition, the smaller the bonus to Duration. Some mages use the Conditional Duration to levy curses designed to teach a target a lesson (“You will suffer boils on your hands until you dirty your hands helping another out of kindness.”), while others employ this Attainment tactically (“This floor will vanish the second I snap this glass rod.”).","Spend a point of Mana to add a Conditional Duration to a spell. Doing so adds factors to the spell’s Duration based on the nature of the condition.\nAn **improbable** condition (one that is unlikely to happen given current conditions) adds a level of Duration.\nAn **infrequent** condition (one that will eventually happen, but does not happen often on its own) adds two levels of Duration.\nA **common** condition (one that will almost certainly happen in the near future) adds three levels of Duration.\nWhen the condition is met, the spell ends regardless of how much Duration remains.") },
			{ new Attainment("Precise Force","forces",2,"The mage understands the intricacies of Forces to such a degree that she can optimize their intentional application, perfectly directing her energy when striking an object with a mundane attack or spell that involves a physical projectile.", "If the mage has a full turn to calculate her action, she can take the 9-Again quality on the roll. If she’s applying force to a stationary object, she can ignore two points of Durability, and a successful hit automatically causes two additional Structure damage. Against a stationary, armored target, this strike destroys (and ignores) 1/1 armor if successful. This Attainment doesn’t work against anything moving faster than a casual walk.") },
			{ new Attainment("Improved Pattern Restoration","life",2,"All mages can spend Mana to heal wounds, but an Apprentice of Life can use that Mana more efficiently, healing more or more serious wounds with the same amount of energy. In addition, Scouring her Pattern for Mana becomes easier and less detrimental.","Instead of each bashing or lethal wound costing three points of Mana, the mage can heal bashing damage at a rate of one wound per point of Mana, and lethal damage at a rate of one wound per two points of Mana. In addition, if the mage Scours a Physical Attribute, any derived traits based on that Attribute are not affected (for instance, the mage can Scour a dot of Strength without losing a point of Speed).") },
			{ new Attainment("Permanence","matter",2,"Changing an object’s nature and properties is easier than changing the nature of a living being. An Apprentice of Matter need simply make a small investment of energy to an object to make any Matter spell’s effects long-lasting.","The character may spend one Mana instead of using a Reach to use the Advanced Duration spell factor of a spell with Matter as its highest Arcanum.") },
			{ new Attainment("Mind's Eye","mind",2,"The mage can see Goetia, other Astral entities, and beings using supernatural powers to project out of their bodies in Twilight when using Active Mage Sight with Mind. Her Peripheral Mage Sight reacts to even the passive presence of such entities.","The mage detects Goetia and Mental Twilight phenomena with her Periphery, and can automatically see Goetia and projecting beings in Twilight with her Mind Sight. If a Goetia is using a power to hide, it provokes a Clash of Wills. With the expenditure of one point of Mana, the mage can interact with Goetia for a scene. She can speak with them, touch them, and even strike them. However, this renders her vulnerable to their attentions, as well.") },
			{ new Attainment("Universal Counterspell","prime",2,"An Apprentice of Prime understands the formation of spells and the creation of an Imago well enough to attack it on a direct, metaphysical level, allowing her a great deal more defensive capability.","The mage may use Counterspell on any Awakened spell. The player rolls Gnosis + Prime when the character does not know the Arcanum used, or when this would be a higher dice pool than the appropriate Arcanum. The mage may also spend a point of Mana to Counter a spell’s lowest Arcanum rather than its primary Arcanum. For example, a mage with this Attainment Countering a Fate 4, Space 2 spell may pay a point of Mana to roll the Clash of Wills against Gnosis + Space instead of Gnosis + Fate.") },
			{ new Attainment("Sympathetic Range","space",2,"An Apprentice of Space can cast spells using her sympathy to a subject she cannot see. The mage requires a sympathetic connection to the subject, and a Yantra symbolizing that subject to use as a focus for the spell.","To use this Attainment, the mage must be casting a spell at sensory range, use a sympathy Yantra, and spend one Mana. The spell is Withstood by the fragility of the sympathetic connection (p.173), between the mage and her subject, but if the mage does not know the sympathetic name of the subject the Withstand level increases by one.") }
		};
		public static Dictionary<string, SkillType> Skills { get; set; } = new Dictionary<string, SkillType>()
		{
			{"academics",SkillType.Mental },
			{"computers",SkillType.Mental },
			{"crafts",SkillType.Mental },
			{"investigation",SkillType.Mental},
			{"medicine",SkillType.Mental },
			{"occult",SkillType.Mental },
			{"politics",SkillType.Mental},
			{"science",SkillType.Mental},
			{"athletics",SkillType.Physical},
			{"brawl",SkillType.Physical },
			{"drive",SkillType.Physical},
			{"firearms",SkillType.Physical},
			{"larceny",SkillType.Physical},
			{"stealth",SkillType.Physical},
			{"survival",SkillType.Physical},
			{"weaponry",SkillType.Physical},
			{"animal-ken",SkillType.Social },
			{"empathy",SkillType.Social },
			{"expression",SkillType.Social },
			{"intimidation",SkillType.Social },
			{"persuasion",SkillType.Social },
			{"socialize",SkillType.Social },
			{"streetwise",SkillType.Social },
			{"subterfuge",SkillType.Social }
		};
	}

	public enum Damage { bashing = 1, b = 1, lethal =2, l = 2, aggravated = 3, a = 3}
	public enum DamageLong { Bashing = 1, Lethal = 2, Aggravated = 3}
	public enum SkillType { Mental = 3, Physical = 1, Social = 1}
}
