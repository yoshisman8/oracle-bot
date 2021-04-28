using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
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
using System.Text.RegularExpressions;

namespace Oracle.Modules
{
    public class RollerModule : ModuleBase<SocketCommandContext>
    {
        public Utilities Utils { get; set; }


        [Command("Roll"), Alias("R")]
        public async Task Roll(params string[] Expression)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            List<string> Bonuses = new List<string>();
            bool nines = false;
            bool eights = false;

            var queue = new StringBuilder();

            foreach (var x in Expression)
            {
                if(Actor.Macros.TryGetValue(x.ToLower(),out string macro))
                {
                    string[] segements = macro.Split(" ");
                    foreach(var y in segements)
                    {
                        if (int.TryParse(y, out int number2))
                        {
                            Bonuses.Add(y);
                            queue.Append(y + " ");
                        }
                        else if (Actor.Ranks.TryGetValue(y.ToLower(), out int value2))
                        {
                            if (value2 <= 0)
                            {
                                Bonuses.Add("-" + (int)Icons.Skills[y]);
                                queue.Append(y + "(Untrained: " + "-" + (int)Icons.Skills[y] + ") ");
                            }
                            else
                            {
                                Bonuses.Add(value2.ToString());
                                queue.Append(y + "(" + value2 + ") ");
                            }
                        }
                        else if(y.ToLower() == "perception")
                        {
                            Bonuses.Add((Actor.Ranks["wits"] + Actor.Ranks["composure"]).ToString());
                            queue.Append("Perception(" + (Actor.Ranks["wits"] + Actor.Ranks["composure"]) + ")");
                        }
                        else if (y == "+" || y == "-")
                        {
                            Bonuses.Add(y);
                            queue.Append(y + " ");
                        }
                        else if (y.ToLower() == "9s")
                        {
                            nines = true;
                        }
                        else if (y.ToLower() == "8s")
                        {
                            eights = true;
                        }
                    }
                }
                else if (int.TryParse(x, out int number))
                {
                    Bonuses.Add(x);
                    queue.Append(x + " ");
                }
                else if (Actor.Ranks.TryGetValue(x.ToLower(), out int value))
                {
                    if (value <= 0)
                    {
                        Bonuses.Add("-" + (int)Icons.Skills[x]);
                        queue.Append(x + "(Untrained: " + "-" + (int)Icons.Skills[x] + ") ");
                    }
                    else
                    {
                        Bonuses.Add(value.ToString());
                        queue.Append(x + "(" + value + ") ");
                    }
                }
                else if (x.ToLower() == "perception")
                {
                    Bonuses.Add((Actor.Ranks["wits"] + Actor.Ranks["composure"]).ToString());
                    queue.Append("Perception(" + (Actor.Ranks["wits"] + Actor.Ranks["composure"]) + ")");
                }
                else if (x == "+" || x == "-")
                {
                    Bonuses.Add(x);
                    queue.Append(x + " ");
                }
                else if (x.ToLower() == "9s")
                {
                    nines = true;
                }
                else if (x.ToLower() == "8s")
                {
                    eights = true;
                }
            }

            if (Actor.Penalty < 0)
            {
                Bonuses.Add(Actor.Penalty.ToString());
                queue.Append("- Health Penalty(" + Actor.Penalty + ")");
            }
            if (nines && eights)
            {
                await ReplyAsync(Context.User.Mention + ", You can't have Repeating 9s and Repeating 8s on at the same time!");
                return;
            }

            try
            {
                bool fate = false;
                var total = new DataTable().Compute(string.Join(" ", Bonuses), null);
                if(int.TryParse(total.ToString(),out int totalint))
                {
                    if (totalint <= 0)
                    {
                        fate = true;
                    }
                }

                RollResult result = null;
                if (fate)
                {
                    result = Roller.Roll("1d10!e" + (nines ? "9" : "") + (eights ? "8" : ""));
                }
                else
                {
                    result = Roller.Roll("(" + string.Join(" ", Bonuses) + ")d10!e" + (nines ? "9" : "") + (eights ? "8" : ""));
                }
                
                var embed = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithThumbnailUrl(Actor.Avatar)
                .WithTitle(Actor.Name + " makes a roll.");

                int successes = 0;

                var sb = new StringBuilder();

                foreach (var dice in result.Values)
                {
                    switch (dice.DieType)
                    {
                        case DieType.Normal:
                            switch (dice.NumSides)
                            {
                                case 10:
                                    sb.Append(Icons.d10[(int)dice.Value] + " ");
                                    if (dice.Value >= 8) successes++;
                                    if (dice.Value == 1) successes--;
                                    break;
                                default:
                                    sb.Append(dice.Value);
                                    break;
                            }
                            break;
                        case DieType.Special:
                            switch ((SpecialDie)dice.Value)
                            {
                                case SpecialDie.Add:
                                    sb.Append("+ ");
                                    break;
                                case SpecialDie.CloseParen:
                                    sb.Append(") ");
                                    break;
                                case SpecialDie.Comma:
                                    sb.Append(", ");
                                    break;
                                case SpecialDie.Divide:
                                    sb.Append("/ ");
                                    break;
                                case SpecialDie.Multiply:
                                    sb.Append("* ");
                                    break;
                                case SpecialDie.Negate:
                                    sb.Append("- ");
                                    break;
                                case SpecialDie.OpenParen:
                                    sb.Append("( ");
                                    break;
                                case SpecialDie.Subtract:
                                    sb.Append("- ");
                                    break;
                                case SpecialDie.Text:
                                    sb.Append(dice.Data);
                                    break;
                            }
                            break;
                        default:
                            sb.Append(dice.Value + " ");
                            break;
                    }
                }
                string print = "";
                if(successes >= 1 && !fate)
                {
                    print += successes + " Successes!";
                }
                else if(successes <1 && !fate)
                {
                    print += "Failure!";
                }
                else if(successes >= 1 && fate)
                {
                    print += "Dramatic Success!";
                }
                else if(successes <1 && fate)
                {
                    print += "Dramatic Failure!";
                }
                embed.WithDescription(queue.ToString()+"\n"+sb.ToString() + "\n**" + print+"**");
                if (successes > 0) embed.WithColor(Color.Green);
                else embed.WithColor(Color.Red);

                await ReplyAsync(Context.User.Mention, false, embed.Build());
            }
            catch
            {
                await ReplyAsync(Context.User.Mention + ", There was an error parsing your roll. Double check that all attributes listed are valid and match how they're named on your sheet!");
            }
            
        }
        [Command("GRoll"), Alias("GR")]
        public async Task GRoll(params string[] Expression)
        {
            User User = Utils.GetUser(Context.User.Id);
            Actor Actor = User.Active;
            if (Actor == null)
            {
                await ReplyAsync("You have no active characters. Set one using the `Character` command or create one using the `Create` command");
                return;
            }

            List<string> Bonuses = new List<string>();
            bool nines = false;
            bool eights = false;

            var queue = new StringBuilder();
            foreach (var x in Expression)
            {
                if (Actor.Macros.TryGetValue(x.ToLower(), out string macro))
                {
                    string[] segements = macro.Split(" ");
                    foreach (var y in segements)
                    {
                        if (int.TryParse(y, out int number2))
                        {
                            Bonuses.Add(y);
                            queue.Append(y + " ");
                        }
                        else if (Actor.Ranks2.TryGetValue(y.ToLower(), out int value2))
                        {
                            if (value2 <= 0)
                            {
                                Bonuses.Add("-" + (int)Icons.Skills[y]);
                                queue.Append(y + "(Untrained: " + "-" + (int)Icons.Skills[y] + ") ");
                            }
                            else
                            {
                                Bonuses.Add(value2.ToString());
                                queue.Append(y + "(" + value2 + ") ");
                            }
                        }
                        else if (Actor.Ranks.TryGetValue(y.ToLower(), out int value3))
                        {
                            if (value3 <= 0)
                            {
                                Bonuses.Add("-" + (int)Icons.Skills[y]);
                                queue.Append(y + "(Untrained: " + "-" + (int)Icons.Skills[y] + ") ");
                            }
                            else
                            {
                                Bonuses.Add(value3.ToString());
                                queue.Append(y + "(" + value3 + ") ");
                            }
                        }
                        else if (y.ToLower() == "perception")
                        {
                            Bonuses.Add((Actor.Ranks2["wits"] + Actor.Ranks2["composure"]).ToString());
                            queue.Append("Perception(" + (Actor.Ranks2["wits"] + Actor.Ranks2["composure"]) + ")");
                        }
                        else if (y == "+" || y == "-")
                        {
                            Bonuses.Add(y);
                            queue.Append(y + " ");
                        }
                        else if (y.ToLower() == "9s")
                        {
                            nines = true;
                        }
                        else if (y.ToLower() == "8s")
                        {
                            eights = true;
                        }
                    }
                }
                else if (int.TryParse(x, out int number))
                {
                    Bonuses.Add(x);
                    queue.Append(x+" ");
                }
                else if (Actor.Ranks2.TryGetValue(x.ToLower(), out int value))
                {
                    if (value <= 0)
                    {
                        Bonuses.Add("-" + (int)Icons.Skills[x]);
                        queue.Append(x + "(Untrained: " + "-" + (int)Icons.Skills[x] + ") ");
                    }
                    else
                    {
                        Bonuses.Add(value.ToString());
                        queue.Append(x + "(" + value + ") ");
                    }
                }
                else if (Actor.Ranks.TryGetValue(x.ToLower(), out int valueM))
                {
                    if (valueM <= 0)
                    {
                        Bonuses.Add("-" + (int)Icons.Skills[x]);
                        queue.Append(x + "(Untrained: " + "-" + (int)Icons.Skills[x] + ") ");
                    }
                    else
                    {
                        Bonuses.Add(valueM.ToString());
                        queue.Append(x + "(" + valueM + ") ");
                    }
                }
                else if (x.ToLower() == "perception")
                {
                    Bonuses.Add((Actor.Ranks2["wits"] + Actor.Ranks2["composure"]).ToString());
                    queue.Append("Perception(" + (Actor.Ranks2["wits"] + Actor.Ranks2["composure"]) + ")");
                }
                else if (x == "+" || x == "-")
                {
                    Bonuses.Add(x);
                    queue.Append(x + " ");
                }
                else if (x.ToLower() == "9s")
                {
                    nines = true;
                }
                else if (x.ToLower() == "8s")
                {
                    eights = true;
                }
            }

            if(Actor.Penalty < 0)
            {
                Bonuses.Add(Actor.Penalty.ToString());
                queue.Append("- Health Penalty(" + Actor.Penalty + ")");
            }

            if (nines && eights)
            {
                await ReplyAsync(Context.User.Mention + ", You can't have Repeating 9s and Repeating 8s on at the same time!");
                return;
            }

            try
            {
                bool fate = false;
                var total = new DataTable().Compute(string.Join(" ", Bonuses), null);
                if (int.TryParse(total.ToString(), out int totalint))
                {
                    if (totalint <= 0)
                    {
                        fate = true;
                    }
                }

                RollResult result = null;
                if (fate)
                {
                    result = Roller.Roll("1d10!e" + (nines ? "9" : "") + (eights ? "8" : ""));
                }
                else
                {
                    result = Roller.Roll("(" + string.Join(" ", Bonuses) + ")d10!e" + (nines ? "9" : "") + (eights ? "8" : ""));
                }

                var embed = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithThumbnailUrl(Actor.Avatar2)
                .WithTitle(Actor.Name2 + " makes a roll.");

                int successes = 0;

                var sb = new StringBuilder();

                foreach (var dice in result.Values)
                {
                    switch (dice.DieType)
                    {
                        case DieType.Normal:
                            switch (dice.NumSides)
                            {
                                case 10:
                                    sb.Append(Icons.d10[(int)dice.Value] + " ");
                                    if (dice.Value >= 8) successes++;
                                    if (dice.Value == 1) successes--;
                                    break;
                                default:
                                    sb.Append(dice.Value);
                                    break;
                            }
                            break;
                        case DieType.Special:
                            switch ((SpecialDie)dice.Value)
                            {
                                case SpecialDie.Add:
                                    sb.Append("+ ");
                                    break;
                                case SpecialDie.CloseParen:
                                    sb.Append(") ");
                                    break;
                                case SpecialDie.Comma:
                                    sb.Append(", ");
                                    break;
                                case SpecialDie.Divide:
                                    sb.Append("/ ");
                                    break;
                                case SpecialDie.Multiply:
                                    sb.Append("* ");
                                    break;
                                case SpecialDie.Negate:
                                    sb.Append("- ");
                                    break;
                                case SpecialDie.OpenParen:
                                    sb.Append("( ");
                                    break;
                                case SpecialDie.Subtract:
                                    sb.Append("- ");
                                    break;
                                case SpecialDie.Text:
                                    sb.Append(dice.Data);
                                    break;
                            }
                            break;
                        default:
                            sb.Append(dice.Value + " ");
                            break;
                    }
                }
                string print = "";
                if (successes >= 1 && !fate)
                {
                    print += successes + " Successes!";
                }
                else if (successes < 1 && !fate)
                {
                    print += "Failure!";
                }
                else if (successes >= 1 && fate)
                {
                    print += "Dramatic Success!";
                }
                else if (successes < 1 && fate)
                {
                    print += "Dramatic Failure!";
                }
                embed.WithDescription(queue.ToString() + "\n" + sb.ToString() + "\n**" + print + "**");
                if (successes > 0) embed.WithColor(Color.Green);
                else embed.WithColor(Color.Red);

                await ReplyAsync(Context.User.Mention, false, embed.Build());
            }
            catch
            {
                await ReplyAsync(Context.User.Mention + ", There was an error parsing your roll. Double check that all attributes listed are valid and match how they're named on your sheet!");
            }
        }
    }
}
