namespace VoiceMeeter.NET.Configuration.Values;

/// <summary>
/// A <see cref="ICustomEnumSetting"/> corresponds to an ensemble of properties in VoiceMeeter of which only one can be toggled on <br/>
/// Much like we can only have one value of an enum in .NET unless we use them as flags
/// </summary>
public interface ICustomEnumSetting : ICustomConfigurationSetting
{
    /// <summary>
    /// Returns a <see cref="IReadOnlyCollection{T}"/> containing all the possible values of the enumeration
    /// </summary>
    /// <returns></returns>
    IReadOnlyCollection<ICustomConfigurationSetting> GetValues();
}