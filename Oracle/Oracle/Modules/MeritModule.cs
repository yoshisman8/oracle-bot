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
    [Name("Merit"), Alias("Merits", "M")]
    public class MeritModule : ModuleBase<SocketCommandContext>
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
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no merits.");
                return;
            }

            var pages = new List<PageBuilder>();

            foreach (var merit in Actor.Merits.OrderBy(x => x.Name))
            {
                var page = new PageBuilder()
                    .WithTitle(merit.Name + " " + Actor.RenderDots(merit.Ranks))
                    .WithThumbnailUrl(Actor.Avatar);
                foreach (var segment in merit.Description)
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
        public async Task New(string Name, int Ranks, params string[] Fields)
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
                await ReplyAsync(Context.User.Mention + ", Each Merit field cannot exceed more than 1024 characters!");
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
            var merit = new Merit()
            {
                Name = Name,
                Ranks = Ranks,
                Description = Fields
            };
            Actor.Merits.Add(merit);
            Utils.UpdateActor(Actor);

            await ReplyAsync(Context.User.Mention + ", Added Merit **" + Name + "** to " + Actor.Name + "'s merits with " + Ranks + " dots into this new merit.");
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

            if (Actor.Merits.Any(x => x.Name.ToLower().StartsWith(Name.ToLower())))
            {
                Merit M = Actor.Merits.First(x => x.Name.ToLower().StartsWith(Name.ToLower()));
                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to delete **" + Actor.Name + "'s** **" + M.Name + "** merit?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    Actor.Merits.Remove(M);
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Removed merit " + M.Name + " from " + Actor.Name + "'s merits!");
                    return;
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no merit whose name starts with \"" + Name + "\".");
                return;
            }
        }
        [Command("Ranks"), Alias("Set-Ranks", "Dots", "Set-Dots")]
        public async Task Dots(string Name, int Dots)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (Dots<0 || Dots > 10)
            {
                await ReplyAsync(Context.User.Mention + ", You can only set your dots to a value between 0 and 10!");
                return;
            }

            if (Actor.Merits.Any(x => x.Name.ToLower().StartsWith(Name.ToLower())))
            {
                Merit M = Actor.Merits.First(x => x.Name.ToLower().StartsWith(Name.ToLower()));

                int I = Actor.Merits.IndexOf(M);

                Actor.Merits[I].Ranks = Dots;
                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", Set merit " + Actor.Name + "'s " + M.Name + " merits to have " + Dots + "dots.");

            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no merit whose name starts with \"" + Name + "\".");
                return;
            }
        }
    }
    [Name("GMerit"), Alias("GMerits", "GM")]
    public class GMeritModule : ModuleBase<SocketCommandContext>
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

            if (Actor.Merits2.Count == 0)
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name2 + " has no Merits2.");
                return;
            }

            var pages = new List<PageBuilder>();

            foreach (var merit in Actor.Merits2.OrderBy(x => x.Name))
            {
                var page = new PageBuilder()
                    .WithTitle(merit.Name + " " + Actor.RenderDots(merit.Ranks))
                    .WithThumbnailUrl(Actor.Avatar2);
                foreach (var segment in merit.Description)
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
        public async Task New(string Name2, int Ranks, params string[] Fields)
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
                await ReplyAsync(Context.User.Mention + ", Each Merit field cannot exceed more than 1024 characters!");
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
            var merit = new Merit()
            {
                Name = Name2,
                Ranks = Ranks,
                Description = Fields
            };
            Actor.Merits2.Add(merit);
            Utils.UpdateActor(Actor);

            await ReplyAsync(Context.User.Mention + ", Added Merit **" + Name2 + "** to " + Actor.Name2 + "'s Merits2 with " + Ranks + " dots into this new merit.");
        }
        [Command("Remove"), Alias("Delete", "Del", "Rem")]
        public async Task Delete([Remainder] string Name2)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            if (Actor.Merits2.Any(x => x.Name.ToLower().StartsWith(Name2.ToLower())))
            {
                Merit M = Actor.Merits2.First(x => x.Name.ToLower().StartsWith(Name2.ToLower()));
                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to delete **" + Actor.Name2 + "'s** **" + M.Name + "** merit?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    Actor.Merits2.Remove(M);
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Removed merit " + M.Name + " from " + Actor.Name2 + "'s Merits2!");
                    return;
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name2 + " has no merit whose Name2 starts with \"" + Name2 + "\".");
                return;
            }
        }
        [Command("Ranks"), Alias("Set-Ranks", "Dots", "Set-Dots")]
        public async Task Dots(string Name2, int Dots)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (Dots < 0 || Dots > 10)
            {
                await ReplyAsync(Context.User.Mention + ", You can only set your dots to a value between 0 and 10!");
                return;
            }

            if (Actor.Merits2.Any(x => x.Name.ToLower().StartsWith(Name2.ToLower())))
            {
                Merit M = Actor.Merits2.First(x => x.Name.ToLower().StartsWith(Name2.ToLower()));

                int I = Actor.Merits2.IndexOf(M);

                Actor.Merits2[I].Ranks = Dots;
                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", Set merit " + Actor.Name2 + "'s " + M.Name + " Merits2 to have " + Dots + "dots.");

            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name2 + " has no merit whose Name2 starts with \"" + Name2 + "\".");
                return;
            }
        }
    }
}
