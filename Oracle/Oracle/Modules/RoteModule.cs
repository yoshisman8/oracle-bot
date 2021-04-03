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
    [Name("Rote"), Alias("Rotes")]
    public class RoteModule : ModuleBase<SocketCommandContext>
    {
        public LiteDatabase Database { get; set; }
        public Utilities Utils { get; set; }
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

            if (Actor.Merits.Count == 0)
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no rotes.");
                return;
            }

            var pages = new List<PageBuilder>();

            foreach (var rote in Actor.Rotes.OrderBy(x => x.Name))
            {
                var page = new PageBuilder()
                    .WithTitle(rote.Name)
                    .WithThumbnailUrl(Actor.Avatar);
                foreach (var segment in rote.Description)
                {
                    page.AddField("Description", segment);
                }
                pages.Add(page);
            }

            var paginator = new StaticPaginatorBuilder()
                .WithUsers(Context.User)
                .WithPages(pages)
                .WithDefaultEmotes()
                .WithFooter(PaginatorFooter.PageNumber)
                .Build();

            await Interactivity.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
        }
        [Command("New"), Alias("Create","Add")]
        public async Task New(string Name, params string[] Fields)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            if (Fields.Any(x => x.Length > 1024))
            {
                await ReplyAsync(Context.User.Mention + ", Each Rote field cannot exceed more than 1024 characters!");
                return;
            }
            if (Fields.Count() > 20)
            {
                await ReplyAsync(Context.User.Mention + ", You can only have 20 fields!");
                return;
            }
            if (Fields.Length > 5900)
            {
                await ReplyAsync(Context.User.Mention + ", You can only have a total of 5900 characters!");
                return;
            }
            var Rote = new Rote()
            {
                Name = Name,
                Description = Fields
            };
            Actor.Rotes.Add(Rote);
            Utils.UpdateActor(Actor);

            await ReplyAsync(Context.User.Mention + ", Added Rote **" + Name + "** to " + Actor.Name + "/"+Actor.Name2+".");
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

            if (Actor.Rotes.Any(x => x.Name.ToLower().StartsWith(Name.ToLower())))
            {
                Rote M = Actor.Rotes.First(x => x.Name.ToLower().StartsWith(Name.ToLower()));
                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to delete "+ Actor.Name + "/" + Actor.Name2 + "'s **" + M.Name + "** Rote?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    Actor.Rotes.Remove(M);
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Removed Rote **" + Name + "** from " + Actor.Name + "/" + Actor.Name2 + ".");
                    return;
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no Rote whose name starts with \"" + Name + "\".");
                return;
            }
        }
    }
}
