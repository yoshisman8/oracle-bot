using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using LiteDB;
using System.Linq;
using Interactivity;
using Oracle.Services;
using Oracle.Data;
using Interactivity.Pagination;
using Interactivity.Confirmation;

namespace Oracle.Modules
{
    [Name("Macro"),Alias("Macros")]
    public class MacroModule : ModuleBase<SocketCommandContext>
    {
        public Utilities Utils { get; set; }
        public LiteDatabase Database { get; set; }
        public InteractivityService Interactivity { get; set; }

        [Command("")]
        public async Task ViewAll()
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if(Actor.Macros.Count == 0)
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " has no macros.");
                return;
            }
            var eb = new EmbedBuilder()
                .WithTitle(Actor.Name+"/"+Actor.Name2+"'s Macros");
            var sb = new StringBuilder();
            foreach(var macro in Actor.Macros)
            {
                sb.AppendLine("• " + macro.Key + ": `" + macro.Value + "`");
            }
            eb.WithDescription(sb.ToString());

            await ReplyAsync(Context.User.Mention, false, eb.Build());
        }

        [Command("New"), Alias("Create", "Add")]
        public async Task New(string Name, params string[] Body)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            if (Actor.Macros.TryGetValue(Name.ToLower(),out string dummy1))
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " already has a macro with that name!");
                return;
            }

            if(Actor.Ranks.TryGetValue(Name.ToLower(), out int dummy2))
            {
                await ReplyAsync(Context.User.Mention + ", you can't create a macro whose name is the same as one of your character's attributes!");
                return;
            }

            foreach(var x in Body)
            {
                if(!Actor.Ranks.ContainsKey(x.ToLower()) && !int.TryParse(x,out int dummy3) && x != "+" && x != "-" && x.ToLower()!="9s" && x.ToLower() != "8s")
                {
                    await ReplyAsync(Context.User.Mention + ", This macro contains an invalid keyword. Only Attributes, Skills, Arcana, \"9s\", \"8s\", flat numbers and + & - symbols are allowed.");
                    return;
                }
            }

            Actor.Macros.Add(Name.ToLower(),string.Join(" ",Body));
            Utils.UpdateActor(Actor);

            await ReplyAsync(Context.User.Mention + ", Added Macro **" + Name + "** to " + Actor.Name + "/" + Actor.Name2 + ".");
        }
        [Command("Remove"), Alias("Delete", "Del", "Rem")]
        public async Task Delete([Remainder] string Name)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            if (Actor.Macros.Any(x => x.Key.ToLower().StartsWith(Name.ToLower())))
            {
                var M = Actor.Macros.First(x => x.Key.ToLower().StartsWith(Name.ToLower()));
                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to delete " + Actor.Name + "/" + Actor.Name2 + "'s **" + M.Key + "** macro?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    Actor.Macros.Remove(M.Key);
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Removed **" + Name + "** macro from " + Actor.Name + "/" + Actor.Name2 + ".");
                    return;
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " has no macro whose name starts with \"" + Name + "\".");
                return;
            }
        }
    }
}
