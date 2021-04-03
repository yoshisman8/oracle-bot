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
	}
	public enum Damage { bashing = 1, b = 1, lethal =2, l = 2, aggravated = 3, a = 3}
	public enum DamageLong { Bashing = 1, Lethal = 2, Aggravated = 3}
}
