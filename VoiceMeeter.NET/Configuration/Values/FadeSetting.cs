using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using JetBrains.Annotations;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Configuration.Values;

[UsedImplicitly]
public class FadeSetting : AbstractCustomSetting
{
    private static readonly NumberFormatInfo NumberFormatInfo = new NumberFormatInfo()
    {
        NumberDecimalSeparator = "."
    };

    public const double MaxDuration = 120000d;
    public const double MinDuration = 0d;
    
    [Range(IVoiceMeeterResource.MinGain, IVoiceMeeterResource.MaxGain)]
    public float DbValue { get; }

    [Range(MinDuration, MaxDuration)]
    public double Time { get; }

    internal FadeSetting()
    {
        this.DbValue = 0;
        this.Time = 0;
    }
    
    /// <summary>
    /// Create a new instance of a <see cref="FadeSetting"/> object, to be used with FadeBy and FadeTo properties of Buses / Strips
    /// </summary>
    /// <param name="dbValue">The target / offset db value</param>
    /// <param name="duration">The duration of the fade effect</param>
    /// <exception cref="ArgumentOutOfRangeException">If <see cref="duration"/> or <see cref="dbValue"/> are out of range</exception>
    /// <remarks>An implicit conversion exists between <c>(float, TimeSpan)</c> and <see cref="FadeSetting"/></remarks>
    public FadeSetting(float dbValue, TimeSpan duration)
    {
        if (!this.GetType().GetProperty(nameof(this.DbValue))!.GetCustomAttribute<RangeAttribute>()!.IsValid(
                dbValue))
        {
            throw new ArgumentOutOfRangeException(nameof(dbValue), "Should be between -60 and 12");
        }

        if (!this.GetType().GetProperty(nameof(this.Time))!.GetCustomAttribute<RangeAttribute>()!.IsValid(
                duration.TotalMilliseconds))
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Should be between 0 and 120 000");
        }

        this.DbValue = dbValue;
        this.Time = duration.TotalMilliseconds;
    }

    /// <inheritdoc />
    public override object ToVoiceMeeterValue()
    {
        return $"({this.DbValue.ToString("F1", NumberFormatInfo)}, {this.Time.ToString("F0", NumberFormatInfo)})";
    }
    
    /// <inheritdoc />
    public override ParamType ValueType => ParamType.String;

    public static implicit operator FadeSetting((float dbValue, TimeSpan time) valuesTuple)
    {
        return new FadeSetting(valuesTuple.dbValue, valuesTuple.time);
    }
    
    /// <inheritdoc />
    protected override string DebugDisplay()
    {
        return $"({this.DbValue.ToString("F1", NumberFormatInfo)} dB, {this.Time.ToString("F0", NumberFormatInfo)} ms)";
    }
}