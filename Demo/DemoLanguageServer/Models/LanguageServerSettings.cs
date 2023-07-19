namespace DemoLanguageServer.Models;

public class LanguageServerSettings
{
    public int MaxNumberOfProblems { get; set; } = 10;

    public LanguageServerTraceSettings Trace { get; } = new LanguageServerTraceSettings();
}

