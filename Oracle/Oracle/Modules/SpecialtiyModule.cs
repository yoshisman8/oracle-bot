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
    [Name("Specialty"), Alias("Specialties","Specs","Spec")]
    public class SpecModule : ModuleBase<SocketCommandContext>
    {
        public LiteDatabase Database { get; set; }
        public Utilities Utils { get; set; }
        public InteractivityService Interactivity { get; set; }

        [Command("New"), Alias("Create","Add")]
        public async Task New([Remainder] string Name)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            
            Actor.Specialties.Add(Name);
            Utils.UpdateActor(Actor);

            await ReplyAsync(Context.User.Mention + ", Added **" + Name + "** specialty to " + Actor.Name +".");
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

            if (Actor.Specialties.Any(x => x.ToLower().StartsWith(Name.ToLower())))
            {
                string M = Actor.Specialties.First(x => x.ToLower().StartsWith(Name.ToLower()));
                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to delete "+ Actor.Name + "'s **" + M + "** specialty?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    Actor.Specialties.Remove(M);
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Removed **" + Name + "** specialty from " + Actor.Name + ".");
                    return;
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + " has no specialty whose name starts with \"" + Name + "\".");
                return;
            }
        }
    }
    [Name("GSpecialty"), Alias("GSpecialties", "GSpecs","Gspec")]
    public class GSpecModule : ModuleBase<SocketCommandContext>
    {
        public LiteDatabase Database { get; set; }
        public Utilities Utils { get; set; }
        public InteractivityService Interactivity { get; set; }

        [Command("New"), Alias("Create", "Add")]
        public async Task New([Remainder] string Name)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            Actor.Specialties2.Add(Name);
            Utils.UpdateActor(Actor);

            await ReplyAsync(Context.User.Mention + ", Added **" + Name + "** specialty to " + Actor.Name2 + ".");
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

            if (Actor.Specialties2.Any(x => x.ToLower().StartsWith(Name.ToLower())))
            {
                string M = Actor.Specialties2.First(x => x.ToLower().StartsWith(Name.ToLower()));
                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to delete " + Actor.Name2 + "'s **" + M + "** specialty?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    Actor.Specialties2.Remove(M);
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Removed **" + Name + "** specialty from " + Actor.Name2 + ".");
                    return;
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name2 + " has no specialty whose name starts with \"" + Name + "\".");
                return;
            }
        }
    }
}
