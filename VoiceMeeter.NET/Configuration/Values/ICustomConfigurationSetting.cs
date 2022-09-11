using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Configuration.Values;

public interface ICustomConfigurationSetting
{
    /// <summary>
    /// Return the value expected by VoiceMeeter
    /// </summary>
    /// <returns>An object which types match the one specified by <see cref="ValueType"/></returns>
    object ToVoiceMeeterValue();
    
    /// <summary>
    /// Returns the VoiceMeeter <see cref="ParamType"/>
    /// </summary>
    ParamType ValueType { get; }
}