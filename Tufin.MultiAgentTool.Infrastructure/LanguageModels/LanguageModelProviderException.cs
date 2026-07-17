namespace Tufin.MultiAgentTool.Infrastructure.LanguageModels;

public sealed class LanguageModelProviderException : Exception
{
    public LanguageModelProviderException(string message)
        : base(message)
    {
    }

    public LanguageModelProviderException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}