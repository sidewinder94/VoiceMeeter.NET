using JetBrains.Annotations;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Configuration.Values;

public class BusMonoSetting : AbstractCustomSetting
{
    public static readonly BusMonoSetting Off = new(0); 
    public static readonly BusMonoSetting Mono = new(1); 
    public static readonly BusMonoSetting StereoReverse = new(2); 
    
    private readonly int _value;

    private BusMonoSetting(int value)
    {
        this._value = value;
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    private BusMonoSetting(float value)
    {
        this._value = (int)value;
    }

    public override ParamType ValueType => ParamType.Float;
    
    /// <inheritdoc />
    public override object ToVoiceMeeterValue()
    {
        return Convert.ToSingle(this._value);
    }

    protected override string DebugDisplay()
    {
        return this._value switch
        {
            0 => nameof(Off),
            1 => nameof(Mono),
            2 => nameof(StereoReverse),
            _ => "Unknown"
        };
    }
}