namespace MailToLog.Infrastructure.DI;

public class MailToLogOptions
{
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    //public string Model { get; set; } = "qwen2.5:7b";
    public string Model { get; set; } = "qwen2.5-coder:7b";
    public double MinConfidence { get; set; } = 0.7;
    public string ConnectionString { get; set; } = "Data Source=mailtolog.db";
}
