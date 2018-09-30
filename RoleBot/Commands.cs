using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace RoleBot
{
    internal class Commands
    {
        [Command("mention"), Description("Calls out the specified user"), Aliases("fetch", "hail")]
        public async Task Mention(CommandContext ctx, [Description("Calls out the specified user")]
            DiscordMember person)
        {
            await ctx.TriggerTypingAsync();

            if (person.Equals(Bot.Client.GetUserAsync(397454954757095424).Result))
                await ctx.RespondAsync($"Get your ass down here Ni:b::b:a {person.Mention}");

            else if (person.Equals(Bot.Client.GetUserAsync(251505256662302731).Result))
                await ctx.RespondAsync($"Fetch Reinforcements {person.Mention}");

            else if (person.Equals(Bot.Client.GetUserAsync(446831227450949656).Result))
                await ctx.RespondAsync($"Leave your laptop outside {person.Mention}");
            
            else if (person.Equals(Bot.Client.GetUserAsync(400798130091851776).Result))
                await ctx.RespondAsync($"We need a whale with dwarfism {person.Mention}");

            else
                await ctx.RespondAsync($"You are summoned {person.Mention}");
        }

        [Command("bet"), Description("presents the synonyms for the colloquial term \"bet\""),
         Aliases("wager", "bargain")]
        public async Task Bet(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            string[] synonyms =
            {
                "bargain",
                "wager",
                "bet",
                "no u"
            };
            
            var rnd = new Random();
            
            switch (rnd.Next(0,3))
            {
               case 0:
                   await ctx.RespondAsync($"{ctx.User.Mention} {synonyms[0]}");
                   break;
               case 1:
                   await ctx.RespondAsync($"{ctx.User.Mention} {synonyms[1]}");
                   break;
               case 2:
                   await ctx.RespondAsync($"{ctx.User.Mention} {synonyms[2]}");
                   break;
               case 3:
                   await ctx.RespondAsync($"{ctx.User.Mention} {synonyms[3]}");
                   break;
               default:
                   await ctx.RespondAsync($"you thot {ctx.User.Mention}");
                   break;
            }
        }

        [Command("cleanup"), Description("Cleans up messages in the channel"), Aliases("clear", "purge")]
        public async Task Clean(CommandContext ctx, int numberOfMessages)
        {
            var botMessages = from discordMessage in ctx.Channel.GetMessagesAsync(numberOfMessages).Result
                select discordMessage;

            var discordMessages = botMessages.ToList();
            await ctx.Channel.DeleteMessagesAsync(discordMessages);
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"{ctx.Message.Author.Mention} has deleted the last {discordMessages.Count()} messages");
        }
    }
}