using JetBrains.Annotations;

namespace VoiceMeeter.NET.Exceptions;

/// <summary>
/// The exception that is thrown when the generated script provoked an error
/// </summary>
public sealed class VoiceMeeterScriptException : Exception
{
    [UsedImplicitly]
    public string Script { get; }

    public VoiceMeeterScriptException(string message, string script) : base(message)
    {
        this.Script = script;
        this.Data["Script"] = script;
    }
}