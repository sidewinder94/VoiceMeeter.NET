using System.Diagnostics;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Configuration.Values;

[DebuggerDisplay("{DebugDisplay(), nq}")]
public abstract class AbstractCustomSetting : ICustomConfigurationSetting
{
    /// <inheritdoc />
    public abstract object ToVoiceMeeterValue();

    /// <inheritdoc />
    public abstract ParamType ValueType { get; }

    protected virtual string DebugDisplay()
    {
        return this.ToString() ?? this.GetType().Name;
    }
}