using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        DiscordSocketClient client; //봇 클라이언트
        CommandService commands;    //명령어 수신 클라이언트
        /// <summary>
        /// 프로그램의 진입점
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            new Program().BotMain().GetAwaiter().GetResult();   //봇의 진입점 실행
        }

        /// <summary>
        /// 봇의 진입점, 봇의 거의 모든 작업이 비동기로 작동되기 때문에 비동기 함수로 생성해야 함
        /// </summary>
        /// <returns></returns>
        public async Task BotMain()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {    //디스코드 봇 초기화
                LogLevel = LogSeverity.Verbose                              //봇의 로그 레벨 설정 
            });
            commands = new CommandService(new CommandServiceConfig()        //명령어 수신 클라이언트 초기화
            {
                LogLevel = LogSeverity.Verbose                              //봇의 로그 레벨 설정
            });

            //로그 수신 시 로그 출력 함수에서 출력되도록 설정
            client.Log += OnClientLogReceived;
            commands.Log += OnClientLogReceived;

            await client.LoginAsync(TokenType.Bot, "Nzk2NzI0NTc4NDgzMDQ0Mzc0.X_cFlQ.tHkffxEMQl1g6Mig-G6UMzao0Bg"); //봇의 토큰을 사용해 서버에 로그인
            await client.StartAsync();                         //봇이 이벤트를 수신하기 시작

            client.MessageReceived += OnClientMessage;         //봇이 메시지를 수신할 때 처리하도록 설정

            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                        services: null);
            await Task.Delay(-1);   //봇이 종료되지 않도록 블로킹
        }

        private async Task OnClientMessage(SocketMessage arg)
        {
            //수신한 메시지가 사용자가 보낸 게 아닐 때 취소
            var message = arg as SocketUserMessage;
            if (message == null) return;

            int pos = 0;

            //메시지 앞에 !이 달려있지 않고, 자신이 호출된게 아니거나 다른 봇이 호출했다면 취소
            if (!(message.HasCharPrefix('!', ref pos) ||
             message.HasMentionPrefix(client.CurrentUser, ref pos)) ||
              message.Author.IsBot)
                return;

            var context = new SocketCommandContext(client, message);                    //수신된 메시지에 대한 컨텍스트 생성   
            var result = await commands.ExecuteAsync(
                                 context: context,
                                 argPos: pos,
                                 services: null);
        }

        /// <summary>
        /// 봇의 로그를 출력하는 함수
        /// </summary>
        /// <param name="msg">봇의 클라이언트에서 수신된 로그</param>
        /// <returns></returns>
        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());  //로그 출력
            return Task.CompletedTask;
        }
    }

    // 모듈 클래스의 접근제어자가 public이면서 ModuleBase를 상속해야 모듈이 인식된다.
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// !hi 명령어를 입력했을 때 실행되는 함수
        /// </summary>
        [Command("hi")]
        [Alias("hello", "ㅎㅇ")]
        public async Task HelloCommand()
        {
            //ModuleBase를 상속하면 Context 변수를 통해 답장을 보낼 수 있다. 
            await Context.Channel.SendMessageAsync("Hello World!");

            EmbedBuilder eb = new EmbedBuilder();
            
                       //Embed 메시지의 속성 설정
            eb.Color = Color.Blue;          // 메시지의 색을 파란색으로 설정
            eb.Title = "Embed Title";          //Embed의 제목
            eb.Description = "Embed description";    //Embed의 설명
            eb.AddField("필드 1", "필드 1 값");      //필드 선언
            eb.AddField("인라인 필드 1", "인라인 필드 1 값", true);    //인라인 필드 선언
            eb.AddField("인라인 필드 2", "인라인 필드 2 값", true);    //인라인 필드 선언
            
            await Context.Channel.SendMessageAsync("", false, eb.Build());  //Embed를 빌드하여 메시지 전송
        }
    }
}