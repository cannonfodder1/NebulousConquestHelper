using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NebulousConquestHelper
{
    public class BotCommands : BaseCommandModule
    {
        [Command("list_fleets")]
        [Description("List all fleets")]
        [Aliases("lf")]
        public async Task ListFleets(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var emoji = DiscordEmoji.FromName(ctx.Client, ":printer:");

            await ctx.RespondAsync($"{emoji} Listing all the fleets now:");
        }
    }
}
