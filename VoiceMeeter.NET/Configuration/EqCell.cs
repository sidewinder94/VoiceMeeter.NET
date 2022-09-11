using System.ComponentModel.DataAnnotations;
using VoiceMeeter.NET.Attributes;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Configuration;

public class EqCell : VoiceMeeterResource<EqCell>
{
    private readonly ChangeTracker _changeTracker;
    private readonly Eq _eq;
    private readonly int _channel;

    private bool _on;
    private int _type;
    private float _frequency;
    private float _gain;
    private float _quality = 3; // 3 is the default VM value

    public override string ResourceType => "Cell";

    [VoiceMeeterParameter(nameof(_on), "on", ParamType.Bool)]
    public bool On
    {
        get => this._on;
        set => this.SetProperty(ref this._on, value);
    }

    [Range(0, 6)]
    [VoiceMeeterParameter(nameof(_type), "type", ParamType.Integer)]
    public int Type
    {
        get => this._type;
        set => this.SetProperty(ref this._type, value);
    }

    [Range(20, 20_000)]
    [VoiceMeeterParameter(nameof(_frequency), "f", ParamType.Float)]
    public float Frequency
    {
        get => this._frequency;
        set => this.SetProperty(ref this._frequency, value);
    }

    [Range(-12, 12)]
    [VoiceMeeterParameter(nameof(_gain), "gain", ParamType.Float)]
    public float Gain
    {
        get => this._gain;
        set => this.SetProperty(ref this._gain, value);
    }

    [Range(1, 100)]
    [VoiceMeeterParameter(nameof(_quality), "q", ParamType.Float)]
    public float Quality
    {
        get => this._quality;
        set => this.SetProperty(ref this._quality, value);
    }

    public EqCell(ChangeTracker changeTracker, Eq eq, int channel, int cell) : base(changeTracker,
        VoiceMeeterType.VoiceMeeter, cell)
    {
        this._changeTracker = changeTracker;
        this._eq = eq;
        this._channel = channel;
    }

    /// <inheritdoc />
    internal override string GetFullParamName(string paramName)
    {
        return this._eq.GetFullParamName($"channel[{this._channel}].cell[{this.Index}].{paramName}");
    }
}