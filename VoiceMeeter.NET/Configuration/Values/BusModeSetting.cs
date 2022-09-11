using System.Reflection;
using JetBrains.Annotations;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Configuration.Values;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class BusModeSetting : AbstractCustomSetting, ICustomEnumSetting
{
    public static readonly BusModeSetting Normal = new("normal");
    public static readonly BusModeSetting AMix = new("Amix");
    public static readonly BusModeSetting BMix = new("Bmix");
    public static readonly BusModeSetting Repeat = new("Repeat");
    public static readonly BusModeSetting Composite = new("Composite");
    public static readonly BusModeSetting TvMix = new("TVMix");
    public static readonly BusModeSetting UpMix21 = new("UpMix21");
    public static readonly BusModeSetting UpMix41 = new("UpMix41");
    public static readonly BusModeSetting UpMix61 = new("UpMix61");
    public static readonly BusModeSetting CenterOnly = new("CenterOnly");
    public static readonly BusModeSetting LfeOnly = new("LFEOnly");
    public static readonly BusModeSetting RearOnly = new("RearOnly");

    public static readonly BusModeSetting[] KnownValues;
    
    private readonly string _busMode;

    static BusModeSetting()
    {
        KnownValues = typeof(BusModeSetting)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(BusModeSetting))
            .Select(field => field.GetValue(null))
            .Cast<BusModeSetting>()
            .ToArray();
    }

    private BusModeSetting()
    {
        this._busMode = string.Empty;
    }
    
    private BusModeSetting(string busMode)
    {
        this._busMode = busMode;
    }

    /// <inheritdoc />
    public override object ToVoiceMeeterValue()
    {
        return this._busMode;
    }

    /// <inheritdoc />
    public override ParamType ValueType => ParamType.CustomEnum;

    /// <inheritdoc />
    public IReadOnlyCollection<ICustomConfigurationSetting> GetValues()
    {
        return KnownValues;
    }

    /// <inheritdoc />
    protected override string DebugDisplay()
    {
        return this._busMode;
    }
}