using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChatGPTTest
{
    internal class Program
    {
        static List<ChatPrompt> chatPrompts = new();

        static void Main(string[] args)
        {
            Config botConfig = new Config();
            botConfig.InitBotConfig();
            Console.OutputEncoding = Encoding.UTF8;

            chatPrompts.Add(new ChatPrompt("system", botConfig.SystemPrompt));
            Start(new OpenAIClient(botConfig.OpenAIToken)).GetAwaiter().GetResult();
        }

        public static async Task Start(OpenAIClient api)
        {
            do
            {
                string? context;
                do
                {
                    Log.Info("請輸入內容，輸入\"list\"顯示歷史對話紀錄，輸入\"exit\"離開程式: ", false);
                    context = Console.ReadLine();
                } while (string.IsNullOrEmpty(context));

                if (context.Trim().ToLower() == "exit")
                    break;

                else if (context.Trim().ToLower() == "list")
                {
                    Console.WriteLine();
                    Log.Info("顯示對話紀錄");
                    foreach (var item in chatPrompts)
                    {
                        Log.New($"[{item.Role}]: {item.Content}");
                    }
                }
                else
                {
                    var cts = new CancellationTokenSource();
                    var cts2 = new CancellationTokenSource();

                    var mainTask = Task.Run(async () =>
                    {
                        try
                        {
                            await foreach (var item in StartTalk(api, context, cts.Token))
                            {
                                Log.New(item, false);
                                cts2.Cancel();
                            }
                        }
                        catch (Exception)
                        {
                            Log.Error("Talk Timeout");
                        }
                    });

                    var waitingTask = Task.Delay(TimeSpan.FromSeconds(3), cts2.Token);
                    var completedTask = await Task.WhenAny(mainTask, waitingTask);
                    if (completedTask == waitingTask && !waitingTask.IsCanceled)
                    {
                        // 如果等待Task先完成，就取消主要的Task
                        cts.Cancel();
                    }

                    await mainTask;
                }

                Console.WriteLine();
            } while (true);
        }

        public static async IAsyncEnumerable<string> StartTalk(OpenAIClient api, string context, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            chatPrompts.Add(new ChatPrompt("user", context));
            var chatRequest = new ChatRequest(chatPrompts, Model.GPT3_5_Turbo);
            string role = "", completionMessage = "";

            await foreach (var result in api.ChatEndpoint.StreamCompletionEnumerableAsync(chatRequest, cancellationToken))
            {
                Log.Debug(result.ToString());

                if (result?.FirstChoice?.FinishReason != null && result?.FirstChoice?.FinishReason == "stop")
                    break;

                if (result?.FirstChoice?.Delta == null)
                    continue;

                if (result.FirstChoice.Delta.Role != null)
                    role = result.FirstChoice.Delta.Role;

                if (result.FirstChoice.Delta.Content != null)
                {
                    completionMessage += result.FirstChoice.Delta.Content;
                    yield return result.FirstChoice.Delta.Content;
                }
            }

            Console.WriteLine();
            Log.Debug($"Talk Done. Role: {role}, Message: {completionMessage}");

            chatPrompts.Add(new ChatPrompt(role, completionMessage));
        }
    }
}