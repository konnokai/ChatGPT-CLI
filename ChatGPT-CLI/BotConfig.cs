using Newtonsoft.Json;

public class Config
{
    public string OpenAIToken { get; set; } = "sk-";
    public string SystemPrompt { get; set; } = "你是一個有幫助的助手。";

    public void InitBotConfig()
    {
        try { File.WriteAllText("config_example.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented)); } catch { }
        if (!File.Exists("config.json"))
        {
            Log.Error($"config.json遺失，請依照 {Path.GetFullPath("config_example.json")} 內的格式填入正確的數值");
            if (!Console.IsInputRedirected)
                Console.ReadKey();
            Environment.Exit(3);
        }

        try
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"))!;

            if (string.IsNullOrWhiteSpace(config.OpenAIToken))
            {
                Log.Error($"{nameof(OpenAIToken)}遺失，請輸入至config.json後重開程式");
                if (!Console.IsInputRedirected)
                    Console.ReadKey();
                Environment.Exit(3);
            }

            if (!config.OpenAIToken.StartsWith("sk-"))
            {
                Log.Error($"{nameof(OpenAIToken)}格式錯誤，請輸入正確的Open AI Token");
                if (!Console.IsInputRedirected)
                    Console.ReadKey();
                Environment.Exit(3);
            }

            if (string.IsNullOrWhiteSpace(config.SystemPrompt))
            {
                Log.Error($"{nameof(SystemPrompt)}遺失，請輸入至config.json後重開程式");
                if (!Console.IsInputRedirected)
                    Console.ReadKey();
                Environment.Exit(3);
            }

            OpenAIToken = config.OpenAIToken;
            SystemPrompt = config.SystemPrompt;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "設定檔讀取失敗");
            throw;
        }
    }
}