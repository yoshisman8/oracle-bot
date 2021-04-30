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
    public class CharacterModule : ModuleBase<SocketCommandContext>
    {
        public LiteDatabase Database { get; set; }
        public Utilities Utils { get; set; }
        public InteractivityService Interactivity { get; set; }
        
        [Command("Create"),Alias("New")]
        public async Task New(string Name1, string Name2)
        {
            User User = Utils.GetUser(Context.User.Id);
            var col = Database.GetCollection<Actor>("Actors");

            Actor Actor = new Actor()
            {
                Name = Name1,
                Name2 = Name2,
                Owner = Context.User.Id
            };
            Actor.Refresh();
            int index = col.Insert(Actor);
            col.EnsureIndex("Name", "LOWER($.Name)");
            col.EnsureIndex("Name2", "LOWER($.Name2)");
            col.EnsureIndex(x => x.Owner);
            
            Actor = col.FindById(index);

            User.Active = Actor;
            Utils.UpdateUser(User);

            await ReplyAsync(Context.User.Mention + ", Created Actors **" + Actor.Name + "** & **" + Actor.Name2 + "** and assigned them as your active character!");
        }
        [Command("Character"),Alias("Char","Sheet","C")]
        public async Task View()
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            var controls = new Dictionary<IEmote, PaginatorAction>(){ { new Emoji("◀️"), PaginatorAction.Backward}, { new Emoji("▶️"), PaginatorAction.Forward } };
            var paginator = new StaticPaginatorBuilder()
                .WithUsers(Context.User)
                .WithPages(Actor.GetSheet())
                .WithEmotes(controls)
                .Build();

            await Interactivity.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
        }

        [Command("Character"), Alias("Char", "Sheet", "C")]
        public async Task Change([Remainder]string Name)
        {
            User User = Utils.GetUser(Context.User.Id);
            var col = Database.GetCollection<Actor>("Actors");

            var results = col.Find(x => x.Owner == Context.User.Id && (x.Name.StartsWith(Name.ToLower()) || x.Name2.StartsWith(Name.ToLower())));
            
            if(results.Count() == 0)
            {
                await ReplyAsync(Context.User.Mention + ", You have no characters who's Branded or Guardian's name starts with \"" + Name + "\".");
                return;
            }
            else
            {
                Actor actor = results.FirstOrDefault();
                User.Active = actor;
                Utils.UpdateUser(User);
                await ReplyAsync(Context.User.Mention + ", Changed your active characters to **" + actor.Name + "** & **" + actor.Name2 + "**.");
            }
        }
        [Command("Delete"), Alias("Del","Remove","Rem")]
        public async Task Delete([Remainder]string Name)
        {
            User User = Utils.GetUser(Context.User.Id);
            var col = Database.GetCollection<Actor>("Actors");

            var results = col.Find(x => x.Owner == Context.User.Id && (x.Name.StartsWith(Name.ToLower()) || x.Name2.StartsWith(Name.ToLower())));
            if (results.Count() == 0)
            {
                await ReplyAsync(Context.User.Mention + ", You have no characters who's Branded or Guardian's name starts with \"" + Name + "\".");
                return;
            }
            else
            {
                Actor actor = results.FirstOrDefault();

                var request = new ConfirmationBuilder()
                    .WithUsers(Context.User)
                    .WithContent(new PageBuilder().WithText("Are you sure you want to delete **" + actor.Name + "** and **" + actor.Name2 + "**?"))
                    .Build();

                var result = await Interactivity.SendConfirmationAsync(request, Context.Channel, TimeSpan.FromMinutes(1));

                if (result.Value)
                {
                    if(User.Active == actor)
                    {
                        User.Active = null;
                        Utils.UpdateUser(User);
                    }

                    col.Delete(actor.ID);
                    await ReplyAsync(Context.User.Mention + ", Deleted characters **" + actor.Name + "** and **" + actor.Name2 + "**.");
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + ", Cancelled Deletion.");
                }
            }
            
        }
        [Command("Set")]
        public async Task Set(string Field, int newvalue)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            if(Actor.Ranks.TryGetValue(Field.ToLower(),out int value))
            {
                if (newvalue < 0)
                {
                    await ReplyAsync(Context.User.Mention + ", You can only set properties to values above 0!");
                    return;
                }
                Actor.Ranks[Field.ToLower()] = newvalue;

                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", Changed **" + Actor.Name + "**'s " + Field + " property from " + value + " to " + newvalue + "!");
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", \"" + Field + "\" is not a valid property on "+Actor.Name+"'s sheet.");
            }
        }
        [Command("Gset"),Alias("guardian-set")]
        public async Task GSet(string Field, int newvalue)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            if (Actor.Ranks2.TryGetValue(Field.ToLower(), out int value))
            {
                if (newvalue < 0)
                {
                    await ReplyAsync(Context.User.Mention + ", You can only set properties to values above 0!");
                    return;
                }
                Actor.Ranks2[Field.ToLower()] = newvalue;

                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", Changed **" + Actor.Name2 + "**'s " + Field + " property from " + value + " to " + newvalue + "!");
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", \"" + Field + "\" is not a valid property on " + Actor.Name2 + "'s sheet.");
            }
        }
        
        [Command("Restore"), Alias("Refresh")]
        public async Task Refresh()
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            Actor.Refresh();

            Utils.UpdateActor(Actor);
            await ReplyAsync(Context.User.Mention + ", Restored **" + Actor.Name + "/" + Actor.Name2 + "**'s Ether, Willpower and Health.");
        }
        [Command("Avatar")]
        public async Task Avatar([Remainder]string url = null)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                if (Context.Message.Attachments.Any(x => x.Url.IsImageUrl()))
                {
                    string file = Context.Message.Attachments.First(x => x.Url.IsImageUrl()).Url;
                    Actor.Avatar = file;
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Changed **" + Actor.Name + "**'s avatar to the uploaded image.\n**If you ever delete the message with the image, the bot will not be able to display the image anymore!.**");
                }
            }
            else if (url.NullorEmpty())
            {
                return;
            }
            else if (url.IsImageUrl())
            {
                Actor.Avatar = url;
                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", Changed **" + Actor.Name + "**'s avatar url.");
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", This isn't a valid image URL.");
            }
        }
        [Command("GAvatar"), Alias("Guardian-Avatar")]
        public async Task GAvatar([Remainder] string url = null)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (Context.Message.Attachments.Count > 0)
            {
                if (Context.Message.Attachments.Any(x => x.Url.IsImageUrl()))
                {
                    string file = Context.Message.Attachments.First(x => x.Url.IsImageUrl()).Url;
                    Actor.Avatar2 = file;
                    Utils.UpdateActor(Actor);
                    await ReplyAsync(Context.User.Mention + ", Changed **" + Actor.Name2 + "**'s avatar to the uploaded image.\n**If you ever delete the message with the image, the bot will not be able to display the image anymore!.**");
                }
            }
            else if (url.NullorEmpty())
            {
                return;
            }
            else if (url.IsImageUrl())
            {
                Actor.Avatar2 = url;
                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", Changed **" + Actor.Name2 + "**'s avatar url.");
            }
            else
            {
                await ReplyAsync(Context.User.Mention + ", This isn't a valid image URL.");
            }
        }
        [Command("Damage"),Alias("Dmg")]
        public async Task Damage(Damage damage, int amount)
        {
            amount = Math.Abs(amount);
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if(Actor.DamageTrack.Count + amount > Actor.Health)
            {
                amount = Actor.DamageTrack.Count - amount;

                Actor.DamageTrack.AddRange(Enumerable.Repeat((int)damage, amount));
                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** took " + amount + " " + Enum.GetName(typeof(DamageLong), (int)damage) + " damage!");
            }
            else
            {
                Actor.DamageTrack.AddRange(Enumerable.Repeat((int)damage, amount));
                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** took " + amount + " " + Enum.GetName(typeof(DamageLong), (int)damage) + " damage!");
            }
        }
        [Command("Heal")]
        public async Task Heal(Damage damage, int amount)
        {
            amount = Math.Abs(amount);
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            var current = Actor.DamageTrack.Where(x => x == (int)damage).ToList();
            if(current.Count == 0)
            {
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " do not have damage of that type in their health track!");
                return;
            }
            else if (current.Count > amount)
            {
                for(int i = 0; 1 < amount; i++)
                {
                    Actor.DamageTrack.Remove((int)damage);
                }
                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " restore " + amount + " " + Enum.GetName(typeof(DamageLong), (int)damage) + " damage!");
                return;
            }
            else if(current.Count < amount)
            {
                int difference = amount - current.Count;

                for (int i = 0; 1 < difference; i++)
                {
                    Actor.DamageTrack.Remove((int)damage);
                }
                Utils.UpdateActor(Actor);

                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " restore " + amount + " " + Enum.GetName(typeof(DamageLong), (int)damage) + " damage!");
                return;
            }
        }
        [Command("Ether"),Alias("Mana")]
        public async Task Ether(int amount)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (amount == 0)
            {
                return;
            }
            if(amount > 0)
            {
                if ((Actor.Ether + amount) >= Icons.MaxEther[Actor.Ranks["gnosis"]])
                {
                    amount = Icons.MaxEther[Actor.Ranks["gnosis"]] - Actor.Ether;
                }
                Actor.Ether += amount;

                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** regained " + amount + " ether.");
            }
            else if(amount < 0)
            {
                if ((Actor.Ether - amount) <= 0)
                {
                    amount = Actor.Ether;
                }
                Actor.Ether -= amount;

                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** spent " + Math.Abs( amount) + " ether.");
            }
        }
        [Command("Willpower"), Alias("WP","Will")]
        public async Task WillPower(int amount)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (amount == 0)
            {
                return;
            }
            if (amount > 0)
            {
                if ((Actor.Willpower + amount) >= Actor.Ranks["willpower"])
                {
                    amount = Actor.Ranks["willpower"] - Actor.Willpower;
                }
                Actor.Willpower += amount;

                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** regained " + amount + " willpower.");
            }
            else if (amount < 0)
            {
                if (Actor.Willpower - amount <= 0)
                {
                    amount = Actor.Willpower;
                }
                Actor.Willpower -= amount;

                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** spent " + Math.Abs(amount) + " willpower.");
            }
        }
        [Command("RulingArcana"),Alias("StrongArcana","SuperiorArcana","Superior","Ruling","Strong")]
        public async Task StrongArcana(params string[] Arcana)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            Actor.StrongArcana = Arcana;
            Utils.UpdateActor(Actor);
            await ReplyAsync(Context.User.Mention + ", Set " + Actor.Name + "/" + Actor.Name2 + "'s ruling arcana to: " + string.Join(" & ", Arcana) + ".");
        }
        [Command("InferiorArcana"), Alias("WeakArcana", "Weak","Inferior")]
        public async Task WeakArcana(params string[] Arcana)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            Actor.WeakArcana = Arcana;
            Utils.UpdateActor(Actor);
            await ReplyAsync(Context.User.Mention + ", Set " + Actor.Name + "/" + Actor.Name2 + "'s inferior arcana to: " + string.Join(" & ", Arcana) + ".");
        }
        [Command("OrderSkills"),Alias("Order","Skills")]
        public async Task Order(params string[] Skills)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            Actor.OrderSkills = Skills;
            Utils.UpdateActor(Actor);
            await ReplyAsync(Context.User.Mention + ", Set " + Actor.Name +"'s Order Skills to: " + string.Join(" & ", Skills) + ".");
        }
        [Command("Beat"),Alias("Beats")]
        public async Task Beats(int amount)
        {
            amount = Math.Abs(amount);
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (Actor.Beats + amount >= 5)
            {
                Actor.Beats = Actor.Beats + amount;

                double loops = Math.Floor((double)Actor.Beats / 5);

                for (int i = 0; i < loops; i++)
                {
                    Actor.Experience++;
                    Actor.Beats -= 5;
                }
                Actor.Beats = (Actor.Beats + amount) - 5;

                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " gained " + amount + " beats and " + loops + " experience!");
                return;
            }
            else
            {
                Actor.Beats += amount;
                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", " + Actor.Name + "/" + Actor.Name2 + " gained " + amount + " beats!");
                return;
            }
        }
        [Command("Experience"),Alias("Exp")]
        public async Task Exp(int amount) 
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            if (amount == 0)
            {
                return;
            }
            if (amount > 0)
            {
                Actor.Experience += amount;

                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** gained " + amount + " experience.");
            }
            else if (amount < 0)
            {
                Actor.Experience += amount;

                Utils.UpdateActor(Actor);
                await ReplyAsync(Context.User.Mention + ", **" + Actor.Name + "/" + Actor.Name2 + "** spent " + amount + " experience.");
            }
        }
    }
}
