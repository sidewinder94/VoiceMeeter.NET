using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using VoiceMeeter.NET.Configuration.Values;
using VoiceMeeter.NET.Enums;
using VoiceMeeter.NET.Exceptions;

namespace VoiceMeeter.NET.Configuration;

/// <summary>
/// Class responsible for starting polling and pushing changes to VoiceMeeter
/// </summary>
public class ChangeTracker
{
    /// <summary>
    /// A <see cref="Dictionary{TKey,TValue}"/> holding all changed to be applied
    /// </summary>
    private Dictionary<string, string> ChangeStore { get; } = new();
    internal VoiceMeeterClient Client { get; }
    internal IObservable<bool> RefreshEventObservable { get; }
    
    /// <summary>
    /// Returns the <see cref="VoiceMeeterConfiguration"/> associated with this <see cref="ChangeTracker"/>
    /// </summary>
    public VoiceMeeterConfiguration VoiceMeeterConfiguration { get; }

    /// <summary>
    /// Gets or Sets a value defining if configuration changes are applied immediately or only on <see cref="Apply"/>
    /// </summary>
    /// <remarks>If <c>false</c> only the last value for each parameter is saved</remarks>
    [UsedImplicitly]
    public bool AutoApply { get; set; } = true;

    internal ChangeTracker(VoiceMeeterClient client, VoiceMeeterConfiguration voiceMeeterConfiguration,
        TimeSpan? refreshFrequency)
    {
        this.Client = client;
        this.VoiceMeeterConfiguration = voiceMeeterConfiguration;
        
        this.RefreshEventObservable = (refreshFrequency.HasValue
                ? Observable.Interval(refreshFrequency.Value)
                : Observable.Return(DateTimeOffset.Now.UtcTicks))
            .Select(_ => this.Client.IsDirty())
            .Publish()
            .RefCount();;
    }

    /// <summary>
    /// Saves a <see cref="float"/> value, applies immediately unless <see cref="AutoApply"/> is set to <c>false</c>
    /// </summary>
    /// <param name="paramName">The name of the VoiceMeeter parameter to write to</param>
    /// <param name="value">The value to set</param>
    /// <exception cref="VoiceMeeterException">In case of a general / unknown error when applying value</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the parameter name is not known</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">If the client <see cref="IVoiceMeeterClient.Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    public void SaveValue(string paramName, float value)
    {
        if (this.AutoApply)
        {
            this.Client.SetParameter(paramName, value);
            return;
        }

        this.ChangeStore[paramName] = value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Saves a <see cref="string"/> value, applies immediately unless <see cref="AutoApply"/> is set to <c>false</c>
    /// </summary>
    /// <param name="paramName">The name of the VoiceMeeter parameter to write to</param>
    /// <param name="value">The value to set</param>
    /// <exception cref="VoiceMeeterException">In case of a general / unknown error when applying value</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the parameter name is not known</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">If the client <see cref="IVoiceMeeterClient.Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    public void SaveValue(string paramName, string value)
    {
        if (this.AutoApply)
        {
            this.Client.SetParameter(paramName, value);
            return;
        }

        this.ChangeStore[paramName] = '"' + value + '"';
    }

    /// <summary>
    /// Saves a <see cref="bool"/> value, applies immediately unless <see cref="AutoApply"/> is set to <c>false</c>
    /// </summary>
    /// <param name="paramName">The name of the VoiceMeeter parameter to write to</param>
    /// <param name="value">The value to set</param>
    /// <exception cref="VoiceMeeterException">In case of a general / unknown error when applying value</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the parameter name is not known</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">If the client <see cref="IVoiceMeeterClient.Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    public void SaveValue(string paramName, bool value)
    {
        float floatValue = value ? 1 : 0;

        if (this.AutoApply)
        {
            this.Client.SetParameter(paramName, floatValue);
            return;
        }

        this.ChangeStore[paramName] = floatValue.ToString("F0");
    }

    /// <summary>
    /// Saves a <see cref="ParamType.Custom"/> value, applies immediately unless <see cref="AutoApply"/> is set to <c>false</c>
    /// </summary>
    /// <param name="paramName">The name of the VoiceMeeter parameter to write to</param>
    /// <param name="setting">The value to set</param>
    /// <exception cref="VoiceMeeterException">In case of a general / unknown error when applying value</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the parameter name is not known</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <see cref="ParamType"/> of the <see cref="ICustomConfigurationSetting"/> is not known</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">If the client <see cref="IVoiceMeeterClient.Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <exception cref="InvalidOperationException">If <see cref="ParamType"/> of the <see cref="ICustomConfigurationSetting"/> is <see cref="ParamType.Custom"/></exception>
    public void SaveValue(string paramName, ICustomConfigurationSetting setting)
    {
        switch (setting.ValueType)
        {
            case ParamType.Float:
                this.SaveValue(paramName, (float)setting.ToVoiceMeeterValue());
                break;
            case ParamType.String:
                this.SaveValue(paramName, (string)setting.ToVoiceMeeterValue());
                break;
            case ParamType.Bool:
                this.SaveValue(paramName, (bool)setting.ToVoiceMeeterValue());
                break;
            case ParamType.CustomEnum:
                this.SaveValue($"{paramName}.{setting.ToVoiceMeeterValue()}", true);
                break;
            case ParamType.Custom:
                throw new InvalidOperationException();
            default:
                throw new ArgumentOutOfRangeException(nameof(setting), $"{nameof(setting.ValueType)} is not a known value");
        }
    }

    /// <summary>
    /// Applies all saved changes to the connected VoiceMeeter Instance
    /// </summary>
    /// <exception cref="InvalidOperationException">When called whilst  <see cref="AutoApply"/> is set to <code>true</code></exception>
    /// <exception cref="VoiceMeeterScriptException">If the generated script has an error</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">If the client <see cref="IVoiceMeeterClient.Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <exception cref="VoiceMeeterException">In case of a general / unknown error when applying value</exception>
    [UsedImplicitly]
    public void Apply()
    {
        if (this.AutoApply) throw new InvalidOperationException($"Cannot apply when {nameof(this.AutoApply)} is enabled");
        
        var scriptBuilder = new StringBuilder();

        foreach (KeyValuePair<string, string> change in this.ChangeStore)
        {
            scriptBuilder.Append($"{change.Key} = {change.Value}\n");
        }

        this.Client.SetParameters(scriptBuilder.ToString());
        
        this.ClearChanges();
    }

    /// <summary>
    /// Empties the <see cref="ChangeStore"/> of changes to apply
    /// </summary>
    /// <exception cref="InvalidOperationException">When called whilst  <see cref="AutoApply"/> is set to <code>true</code></exception>
    [UsedImplicitly]
    public void ClearChanges()
    {
        if (this.AutoApply) throw new InvalidOperationException($"Cannot clear when {nameof(this.AutoApply)} is enabled");
        
        this.ChangeStore.Clear();
    }

    /// <summary>
    /// Gets a custom value from VoiceMeeter and builds the required object to hold and present it
    /// </summary>
    /// <param name="field">The field in which the value is stored (used to get the concrete type)</param>
    /// <param name="customParamType">The <see cref="ParamType"/> on VoiceMeeter side</param>
    /// <param name="paramName">The name of the parameter to fetch from VoiceMeeter</param>
    /// <returns>A <see cref="ICustomConfigurationSetting"/> who represents the VoiceMeeter value</returns>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <see cref="paramName"/> is unknown by VoiceMeeter</exception>
    /// <exception cref="VoiceMeeterException">In cas of an unexpected error (e.g. structure mismatch)</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <see cref="customParamType"/> is <c>null</c>, <see cref="ParamType.Custom"/> or an unknown value</exception>
    /// <exception cref="EntryPointNotFoundException">If the real class implementing <see cref="ICustomConfigurationSetting"/> does not provide a private constructor taking a parameter of the required type</exception>
    public ICustomConfigurationSetting GetCustomParameter(
        FieldInfo field, 
        ParamType? customParamType,
        string paramName)
    {
        object remoteValue;
        Type remoteValueType;

        switch (customParamType)
        {
            case ParamType.Float:
                remoteValue = this.Client.GetFloatParameter(paramName);
                remoteValueType = typeof(float);
                break;
            case ParamType.String:
                remoteValue = this.Client.GetStringParameter(paramName);
                remoteValueType = typeof(string);
                break;
            case ParamType.Bool:
                remoteValue = this.Client.GetFloatParameter(paramName) != 0;
                remoteValueType = typeof(bool);
                break;
            case ParamType.CustomEnum:
                return this.GetCustomEnumValue(paramName, field.FieldType);
            case null:
            case ParamType.Custom:
            default:
                throw new ArgumentOutOfRangeException(nameof(customParamType), customParamType, null);
        }

        var constructor = field.FieldType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, new[] { remoteValueType });

        if (constructor == null)
        {
            throw new EntryPointNotFoundException(
                $"Could not find an appropriate constructor for type {field.FieldType}");
        }
        
        return (ICustomConfigurationSetting)constructor.Invoke(new[] {remoteValue});
    }

    private ICustomConfigurationSetting GetCustomEnumValue(string paramName, Type fieldType)
    {
        var customEnum = (ICustomEnumSetting?)Activator.CreateInstance(fieldType, nonPublic: true);

        if (customEnum == null)
        {
            throw new EntryPointNotFoundException(
                $"Could not find a parameterless constructor for type {fieldType.Name}");
        }
        
        foreach (var value in customEnum.GetValues())
        {
            if (this.Client.GetFloatParameter($"{paramName}.{value.ToVoiceMeeterValue()}") != 0)
            {
                return value;
            }
        }
        
        throw new VoiceMeeterException($"{paramName} value is undefined");
    }
}