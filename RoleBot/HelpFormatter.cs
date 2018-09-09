using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DSharpPlus;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext;

namespace RoleBot
{
    public class HelpFormatter : IHelpFormatter
    {
        private StringBuilder HelpBuilder { get; }

        public HelpFormatter() => HelpBuilder = new StringBuilder();

        public IHelpFormatter WithCommandName(string name)
        {
            HelpBuilder.Append("Command: ").AppendLine(Formatter.Bold(name));
            return this;
        }

        public IHelpFormatter WithDescription(string description)
        {
            HelpBuilder.Append("Description: ").AppendLine(description);
            return this;
        }

        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            HelpBuilder.Append("Arguments: ").AppendLine(string.Join(", ", arguments.Select(xarg => $"{xarg.Name} ({xarg.Type.ToUserFriendlyName()})")));

            return this;
        }

        public IHelpFormatter WithAliases(IEnumerable<String> aliases)
        {
            HelpBuilder.Append("Aliases: ").AppendLine(string.Join(",", aliases));
            return this;
        }

        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            HelpBuilder.Append("Subcommands: ").AppendLine(string.Join(", ", subcommands.Select(xc => xc.Name)));

            return this;
        }

        public IHelpFormatter WithGroupExecutable()
        {
            HelpBuilder.AppendLine("This group is a standalone command.");
            return this;
        }

        public CommandHelpMessage Build()
        {
            return new CommandHelpMessage(HelpBuilder.ToString().Replace("\r\n", "\n"));
        }

    }
}