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
    [Name("Inventory"), Alias("I","Items","Gear","Equipment")]
    public class InventoryModule : ModuleBase<SocketCommandContext>
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
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no Items.");
                return;
            }

            var pages = new List<PageBuilder>();

            foreach (var item in Actor.Inventory.OrderBy(x => x.Name))
            {
                var page = new PageBuilder()
                    .WithTitle(item.Name)
                    .WithThumbnailUrl(Actor.Avatar);
                foreach (var segment in item.Description)
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
                await ReplyAsync(Context.User.Mention + ", Each item field cannot exceed more than 1024 characters!");
                return;
            }
            if(Fields.Count() > 20)
            {
                await ReplyAsync(Context.User.Mention + ", You can only have 20 fields!");
                return;
            }
            if(Fields.Length > 5900)
            {
                await ReplyAsync(Context.User.Mention + ", You can only have a total of 5900 characters!");
                return;
            }
            var Rote = new Item()
            {
                Name = Name,
                Description = Fields
            };
            Actor.Inventory.Add(Rote);
            Utils.UpdateActor(Actor);

            await ReplyAsync(Context.User.Mention + ", Added Item **" + Name + "** to " + Actor.Name + "/"+Actor.Name2+"'s Inventory.");
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

            if (Actor.Inventory.Any(x => x.Name.ToLower().StartsWith(Name.ToLower())))
            {
                Item M = Actor.Inventory.First(x => x.Name.ToLower().StartsWith(Name.ToLower()));
                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to the item **" + M.Name + "'** from" + Actor.Name + "/" + Actor.Name2 + "'s inventory?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    Actor.Inventory.Remove(M);
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Removed item **" + Name + "** from " + Actor.Name + "/" + Actor.Name2 + "'s Inventory.");
                    return;
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no Item whose name starts with \"" + Name + "\".");
                return;
            }
        }
    }
}
