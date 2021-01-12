using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiscordBot
{
    class Program
    {
        DiscordSocketClient client; //봇 클라이언트
        CommandService command;    //명령어 수신 클라이언트
        static void Main(string[] args)
        {
            new Program().BotMain().GetAwaiter().GetResult();
        }

        public async Task BotMain()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });
            command = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            client.Log += OnClientLogReceived;
            command.Log += OnClientLogReceived;

            await client.LoginAsync(TokenType.Bot, "Bruh");
            await client.StartAsync();

            client.MessageReceived += OnClientMessage;

            await command.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                        services: null);

            await Task.Delay(-1);
        }

        private async Task OnClientMessage(SocketMessage arg)
        {
            //수신한 메시지가 사용자가 보낸 게 아닐 때 취소
            SocketUserMessage message = arg as SocketUserMessage;
            if (message == null) return;

            int pos = 0;

            if (!(message.HasCharPrefix('!', ref pos) ||
             message.HasMentionPrefix(client.CurrentUser, ref pos)) ||
              message.Author.IsBot)
                return;

            var context = new SocketCommandContext(client, message);
            var result = await command.ExecuteAsync(
                                 context: context,
                                 argPos: pos,
                                 services: null);
        }

        private Task OnClientLogReceived(LogMessage msg)
        {
            if (msg.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{msg.Severity}] {cmdException.Command.Aliases}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{msg.Severity}] {msg}");

            return Task.CompletedTask;
        }
    }

    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        // [Alias("도움말", "h")]
        public async Task HelloCommand()
        {
            await Context.Channel.SendMessageAsync("Showing Help Message");
        }
    }
}