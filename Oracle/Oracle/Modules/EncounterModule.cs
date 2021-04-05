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
using Dice;

namespace Oracle.Modules
{
    [Name("Encounter"),Alias("Enc","Battle")]
    [RequireContext(ContextType.Guild)]
    public class EncounterModule : ModuleBase<SocketCommandContext>
    {
        public LiteDatabase Database { get; set; }
        public Utilities Utils { get; set; }
        public InteractivityService Interactivity { get; set; }

        [Command("")]
        [RequireContext(ContextType.Guild)]
        public async Task View()
        {
            var col = Database.GetCollection<Encounter>("Encounters");

            if(!col.Exists(x=>x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);

            if (Encounter.Active)
            {
                await ReplyAsync(" ", false, Encounter.GetEncounter(Database));
            }
            else
            {
                await ReplyAsync("There's no active encounter in this channel!");
            }
        }

        [Command("Start"), Alias("Begin","Create")]
        [RequireContext(ContextType.Guild)]
        public async Task StartEncounter()
        {
            var col = Database.GetCollection<Encounter>("Encounters");

            if (!col.Exists(x => x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);
            if (!Encounter.Active)
            {
                Encounter.Active = true;
                Encounter.Participants = new List<Participant>();
                Encounter.Current = null;
                Encounter.Owner = Context.User.Id;
                Encounter.First = true;

                col.Update(Encounter);

                await ReplyAsync(Context.User.Mention + ", Started encounter in the room! Use the `Encounter Join` and `Encounter GJoin` commands to join this encounter using your active Human or Guardian character!."+
                    "\nUse the `Encounter AddNPC <Name> <Initiative>` command to add NPCs to this encounter.");
                return;
            }
            else if (Encounter.Active && !Encounter.Started && Encounter.Owner == Context.User.Id)
            {
                var embed = Encounter.Start(Database);
                col.Update(Encounter);

                ulong[] parts = Encounter.Participants.Select(x => x.Player).Distinct().ToArray();
                string[] mentions = new string[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    mentions[i] = Context.Guild.GetUser(parts[i]).Mention;
                }
                
                await ReplyAsync(string.Join(", ",mentions)+ "\n***Action declaration time!***", false, embed);
                return;
            }
            else if(Encounter.Active && Encounter.Started)
            {
                await ReplyAsync("There's already an encounter happening in this room! Use `Encounter End` to end the current encounter first!");
                return;
            }
            else
            {
                await ReplyAsync("You're not the game master of this encounter!");
                return;
            }
        }
        [Command("End"),Alias("Stop")]
        [RequireContext(ContextType.Guild)]
        public async Task End()
        {
            var col = Database.GetCollection<Encounter>("Encounters");

            if (!col.Exists(x => x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);

            var request = new ConfirmationBuilder()
                .WithContent(new PageBuilder().WithText("Are you sure you want to end the current encounter?"));

            var result = await Interactivity.SendConfirmationAsync(request.Build(), Context.Channel, TimeSpan.FromMinutes(1));

            if (result.Value)
            {
                Encounter.End();
                col.Update(Encounter);
                await ReplyAsync("Encounter ended!");
            }
            else
            {
                await ReplyAsync("Encounter resumed.");
            }
        }
        [Command("Join")]
        [RequireContext(ContextType.Guild)]
        public async Task Join(int bonus = 0)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            var col = Database.GetCollection<Encounter>("Encounters");

            if (!col.Exists(x => x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);

            if (!Encounter.Active)
            {
                await ReplyAsync("There's no active encounter in this room.");
                return;
            }
            var result = Roller.Roll("1d10 + " + Actor.Ranks["composure"] + " + " + Actor.Ranks["dexterity"] + " +" + bonus);

            decimal tiebreaker = Roller.Roll("1d10").Value / 10;

            var feedback = Encounter.Add(Actor.Name, result.Value+tiebreaker, Actor.Owner,Data.Type.Human,Actor.ID);

            col.Update(Encounter);

            await ReplyAsync(Context.User.Mention + ", " + feedback);
        }
        [Command("GJoin")]
        [RequireContext(ContextType.Guild)]
        public async Task GJoin(int bonus = 0)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }
            var col = Database.GetCollection<Encounter>("Encounters");

            if (!col.Exists(x => x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);

            if (!Encounter.Active)
            {
                await ReplyAsync("There's no active encounter in this room.");
                return;
            }
            var result = Roller.Roll("1d10 + " + Actor.Ranks2["composure"] + " + " + Actor.Ranks2["dexterity"] + " +" + bonus);

            decimal tiebreaker = Roller.Roll("1d10").Value / 10;

            var feedback = Encounter.Add(Actor.Name2, result.Value+tiebreaker, Actor.Owner, Data.Type.Guardian, Actor.ID);

            col.Update(Encounter);

            await ReplyAsync(Context.User.Mention + ", " + feedback);
        }
        [Command("Add")]
        [RequireContext(ContextType.Guild)]
        public async Task Add(string Name, int Initiative)
        {
            var col = Database.GetCollection<Encounter>("Encounters");

            if (!col.Exists(x => x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);

            if (!Encounter.Active)
            {
                await ReplyAsync("There's no active encounter in this room.");
                return;
            }
            
            if(Encounter.Owner != Context.User.Id)
            {
                await ReplyAsync("Only the game master of this encounter can add NPCs to initiative.");
                return;
            }

            decimal tiebreaker = Roller.Roll("1d10").Value / 10;
            var feedback = Encounter.Add(Name, Initiative+tiebreaker, Context.User.Id, Data.Type.NPC);

            col.Update(Encounter);

            await ReplyAsync(Context.User.Mention + ", " + feedback);
        }
        [Command("Remove"),Alias("Delete","Rem","Del")]
        [RequireContext(ContextType.Guild)]
        public async Task Delete([Remainder]string Name)
        {
            var col = Database.GetCollection<Encounter>("Encounters");

            if (!col.Exists(x => x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);

            if (!Encounter.Active)
            {
                await ReplyAsync("There's no active encounter in this room.");
                return;
            }

            if (Encounter.Owner != Context.User.Id)
            {
                await ReplyAsync("Only the game master of this encounter can removes participants from initiative.");
                return;
            }

            var result = Encounter.Remove(Name);
            
            if(result== null)
            {
                await ReplyAsync("There is no participant in initiative whose name starts with \"" + Name + "\".");
                return;
            }
            else
            {
                col.Update(Encounter);
                await ReplyAsync(Context.User.Mention + ", " + result);
            } 
        }
        [Command("Next"), Alias("Turn")]
        [RequireContext(ContextType.Guild)]
        public async Task Next()
        {
            var col = Database.GetCollection<Encounter>("Encounters");

            if (!col.Exists(x => x.Channel == Context.Channel.Id))
            {
                Encounter E = new Encounter()
                {
                    Channel = Context.Channel.Id
                };
                col.Insert(E);
            }

            var Encounter = col.FindOne(x => x.Channel == Context.Channel.Id);

            if (!Encounter.Active)
            {
                await ReplyAsync("There's no active encounter in this room.");
                return;
            }
            if (!Encounter.Started)
            {
                await ReplyAsync("The Encounter has not started yet!");
                return;
            }
            if(Encounter.Current.Player!= Context.User.Id && Encounter.Owner != Context.User.Id)
            {
                await ReplyAsync("It's not your turn!");
                return;
            }
            var p = Encounter.Next();

            col.Update(Encounter);

            var user = Context.Guild.GetUser(p.Player);
            if (Encounter.First)
            {
                ulong[] parts = Encounter.Participants.Select(x => x.Player).Distinct().ToArray();
                string[] mentions = new string[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    mentions[i] = Context.Guild.GetUser(parts[i]).Mention;
                }
                await ReplyAsync(string.Join(", ", mentions) + ".\n**Action declaration time!***", false, Encounter.GetEncounter(Database));
            }
            else
            {
                await ReplyAsync(user.Mention + ", it's " + Encounter.Current.Name + "'s turn!", false, Encounter.GetEncounter(Database));
            }
        }
    }
}
