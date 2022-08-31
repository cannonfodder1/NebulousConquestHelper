using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NebulousConquestHelper.src.bot
{
    public class BotHelpFormatter : BaseHelpFormatter
    {
        private StringBuilder MessageBuilder { get; }

        public BotHelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.MessageBuilder = new StringBuilder();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            MessageBuilder.Append("Command: ")
                .AppendLine(Formatter.Bold(command.Name))
                .AppendLine();

            MessageBuilder.Append("Description: ")
                .AppendLine(command.Description)
                .AppendLine();

            if (command is CommandGroup)
            {
                MessageBuilder.AppendLine("This group has a standalone command.").AppendLine();
            }

            MessageBuilder.Append("Aliases: ")
                .AppendLine(string.Join(", ", command.Aliases))
                .AppendLine();

            foreach (CommandOverload overload in command.Overloads)
            {
                if (overload.Arguments.Count == 0)
                {
                    continue;
                }

                MessageBuilder.Append($"[Overload {overload.Priority}] Arguments: ")
                    .AppendLine(string.Join(", ", overload.Arguments.Select(xarg => $"{xarg.Name} ({xarg.Type.Name})")))
                    .AppendLine();
            }

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            MessageBuilder.Append("Subcommands: ")
                .AppendLine(String.Join(", ", subcommands.Select(xc => xc.Name)))
                .AppendLine();
            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(MessageBuilder.ToString().Replace("\r\n", "\n"));
        }
    }
}
