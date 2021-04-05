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
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        public CommandHandlingService CommandHandlingService { get; set; }
        public LiteDatabase Database { get; set; }

        [Command("Prefix")] [RequireUserPermission(GuildPermission.ManageChannels)] [RequireContext(ContextType.Guild)]
        public async Task Prefix([Remainder]string Prefix)
        {
            var guild = CommandHandlingService.GetOrCreateServer(Context.Guild.Id);

            guild.Prefix = Prefix[0].ToString();

            var col = Database.GetCollection<Server>("Servers");

            col.Update(guild);

            await ReplyAsync(Context.User.Mention + ", Changed prefix for this server to `" + guild.Prefix + "`.");
        }

        [Command("Help"),Alias("Command","Commands")]
        public async Task AllHelp()
        {
            string prefix = "!";
            if (Context.Guild != null)
            {
                var guild = CommandHandlingService.GetOrCreateServer(Context.Guild.Id);
                prefix = guild.Prefix;
            }

            var eb = new EmbedBuilder()
                .WithTitle("All Commands")
                .AddField("Character Management", "`" + prefix + "Create <Human Name> <Guardian Name>` - Creates a new character. Each name **must** be encased in quotation marks.\n" +
                "`" + prefix + "Delete <Human or Guardian name>` - Deletes a character. \n" +
                "`" + prefix + "Character [Name]` - Shows your current active character's sheet. Or change your active characters if you input a name.\n")
                .AddField("Character Creation","`"+prefix+"Set <Field> <Value>` - Sets an attribute, skill, arcana, etc on your current active *human* character. To see a list of all valid fields, use the `"+prefix+"help set` command.\n"+
                "`" + prefix +"GSet <field> <Name>` - Same as the Set command, but for your current active *Guardian* character. Certain fields such as Arcana.\n"+
                "`" + prefix +"Avatar [Image URL]` - Sets the avatar for your current active human character. You can use this command while sending an image to instead set the avatar to the uploaded image rather than an image url.\n"+
                "`" + prefix +"GAvatar [Image URL]` - As Avatar, but for your current active Guardian character.\n"+
                "`" + prefix +"SuperiorArcana <Arcanas>` - Sets your superior Arcana. Each arcana must be separated by a space.\n"+
                "`" + prefix +"InferiorArcana <Arcanas>` - Sets your Inferior arcana. Each arcana must be separated by a space.\n"+
                "`" + prefix +"OrderSkills <Skills>` - Sets your order skills for your character. Each skill must be separated by a space.\n")
                .AddField("Merits","`"+prefix+"Merits` - View all your merits. Each page contains one merit.\n"+
                "`" + prefix +"Merits Add <Name> <Initial Dots> <Segments, each encased in quotation marks.>` - Creates a new merit on your *human* character. Each segement **must** be encased in quotation marks. Each segment must note exceed 1024 characters.\n"+
                "`" + prefix +"Merits Dots <Name> <Dots>` - Change the number of Dots you have into a merit.\n"+
                "`" + prefix +"Merits Delete <Name>` - Deletes a Merit from your human character.\n"+
                "`" + prefix +"GMerits` - As Merits, but for your guardian.\n"+
                "`" + prefix +"GMerits Add <Name> <Initial Dots> <Segments>` - As the Merits Add command, but for your guardian.\n"+
                "`" + prefix +"GMerits Dots <Name> <Dots>` - As Merits Dots, but for your guardian.\n"+
                "`" + prefix +"GMerits Delete <Name>` - As Merits Delete, but for your guardian.")
                .AddField("Rotes","`"+prefix+"Rotes` - View all rotes. Each page contains one Rote.\n" +
                "`" + prefix + "Rotes Add <Name> <Segments, each encased in quotation marks.>` - Adds a new rote to your active character. Each segment **must** be encased in quotation marks. Each segment must note exceed 1024 characters.\n"+
                "`" + prefix +"Rotes Delete <Name>` - Deletes a rote from your active character.")
                .AddField("Specializations", "`" + prefix + "Specs Add <Name>` - Adds a specialty to your active human character.\n"+
                "`" + prefix + "Specs Delete <Name>` - Deletes a specialty from your active human character.\n" +
                "`" + prefix + "GSpecs Add <Name>` - Adds a specialty to your active guardian character.\n" +
                "`" + prefix + "GSpecs Delete <Name>` - Deletes a specialty from your active guardian character.")
                .AddField("Inventory", "`" + prefix + "Items` - View all items. Each page contains one Item.\n" +
                "`" + prefix + "Items Add <Name> <Segments, each encased in quotation marks.>` - Adds a new item to your active character's Inventory. Each segment **must** be encased in quotation marks. Each segment must note exceed 1024 characters.\n" +
                "`" + prefix + "Items Delete <Name>` - Deletes an item from your active character.")
                .AddField("Gameplay", "`" + prefix + "Damage <Type> <Ammount>` - Take an ammount of damage. Type can be `Bashing`/`B`, `Lethal`/`L` or `Aggravated`/`A`.\n"+
                "`" + prefix + "Heal <Type> <Ammount>` - Heals a specific type of damage by the indicated amount. Uses the same types as the Damage command.\n"+
                "`" + prefix + "Willpower <+x/-x>` - Uses or regains an amount of willpower.\n"+
                "`" + prefix + "Ether <+x/-x>` - Uses or regains an amount of Ether.\n"+
                "`" + prefix + "Restore` - Restores your active character's Health, Willpower and Ether back to maximum.")
                .WithDescription("Parameters encased with `<>` are mandatory while paramaters encased with `[]` are optional.\nIf you wish to input a parameter with spaces (Such as a name + last name) you'll have to encase it in quotation marks. Ex: `!Create \"Ethan Lockwood\" Estiebus`.");

            var DMs = await Context.User.GetOrCreateDMChannelAsync();
            try
            {
                await DMs.SendMessageAsync("Here's a list of all commands!", false, eb.Build());
            }
            catch
            {
                await ReplyAsync("I couldn't send you the command list because your Direct Messages are disabled!");
            }

        }
        [Command("Help")]
        public async Task HelpDetails(Topics topic)
        {

            string prefix = "!";
            if (Context.Guild != null)
            {
                var guild = CommandHandlingService.GetOrCreateServer(Context.Guild.Id);
                prefix = guild.Prefix;
            }

            EmbedBuilder eb = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithTitle("More help on topic: "+topic);
            switch (topic)
            {
                case Topics.Merits:
                    eb.WithDescription("This commands allow you to add, edit and delete merits from your guardian or human characters. `" + prefix + "Merit` is used for your human character while `" + prefix + "GMerit` is used for your guardian.")
                        .AddField("Segments","A merit is built off 1024 character segments which are all displayed in order. Each segment **Must** be encased in quotation marks (`\"`). You can have up to 20 segments in a single Merit.")
                        .AddField("Example", "Merit Add \"Sleight of Hand\" \"**Prerequisite**: Larceny •••\n**Effect**: Your character can pick locks and pockets without even thinking about it.She can take one Larceny - based instant action reflexively in a given turn.As well, her Larceny actions go unnoticed unless someone is trying specifically to catch her.\"");
                    break;
                case Topics.Rotes:
                    eb.WithDescription("This commands allow you to add, edit and delete Rotes from your active character.")
                        .AddField("Segments", "A rote is built off 1024 character segments which are all displayed in order. Each segment **Must** be encased in quotation marls (`\"`). You can have up to 20 segments in a single Rote.")
                        .AddField("Example", "Rotes Add \"Honing the Form\" \"**Arcana**: Life •••\n**Practice**: Perfecting\n**Primary Factor**: Duration\n**Suggested Rote Skills**: Athletics, Medicine, Survival\"\n\"The mage may improve the subject’s Physical Attributes.The spell increases Strength, Dexterity, or Stamina (chosen when the spell is cast) by its Potency. This increase affects any Advantages or other traits derived from the Attribute’s level.The effects are subtle in appearance; the affected target doesn’t grow or gain any obvious muscle mass, but observers can detect even subtle hints of changes to balance, strength, or stamina.The affected Attribute cannot be raised above the subject’s maximum Attribute dots(5 for normal human beings).\"\n"+
                        "\"**+ 1 Reach**: The spell affects an additional Attribute, dividing the spell’s Potency between both. This effect may be applied twice to affect all three Attributes.\n"+
                        "**+ 1 Reach**: By spending a point of Mana, the mage may raise an Attribute above the maximum rating for the subject.\"");
                    break;
                case Topics.Set:
                    eb.WithDescription("This comman allows you to set different fields on your sheet. The `" + prefix + "Set` is used for your human character while `" + prefix + "GSet` is used for your guardian.")
                        .AddField("Fields", "This is a list of all valid field that are available to the Human and the Guardian:\n**Attributes**\nIntelligence, Wits, Resolve, Strength, Dexterity, Stamina, Presence, Manipulation, Composure.\n" +
                        "**Skills**:\nAcademics, Computers, Crafts, Investigation, Medicine, Occult, Politics, Science, Athletics, Brawl, Drive, Firearms, Larceny, Stealth, Survival, Weaponry, Animal-Ken, Empathy, Expression, Intimidation, Persuasion, Socialize, Streetwise, Subterfuge.\n"+
                        "**Statistics**:\nSize, Armor, Ballistic-Armor.\n"+
                        "**Satistics (Exclusive to the Human)**:\nGnosis, Wisdom, Willpower.\n"+
                        "**Arcana (Exclusive to the Human)**:\nDeath, Mind, Fate, Prime, Forces, Space, Life, Spirit, Matter, Time.");
                    break;
            }

            var DMs = await Context.User.GetOrCreateDMChannelAsync();
            try
            {
                await DMs.SendMessageAsync("Here's more info in regards to "+topic+"!", false, eb.Build());
            }
            catch
            {
                await ReplyAsync("I couldn't send you the information because your Direct Messages are disabled!");
            }

        }
        public enum Topics { Set, Merits, Rotes }
    }
}
    