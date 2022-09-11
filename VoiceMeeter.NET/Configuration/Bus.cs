using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using VoiceMeeter.NET.Attributes;
using VoiceMeeter.NET.Configuration.Values;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Configuration;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Bus : VoiceMeeterResource<Bus>
{
    private BusMonoSetting _mono = BusMonoSetting.Off;
    private bool _mute;
    private bool _isEqEnabled;
    private float _gain;
    private string _deviceName = string.Empty;
    private float _sampleRate;
    private bool _isEqBEnabled;
    private FadeSetting _fadeBy = new();
    private FadeSetting _fadeTo = new();
    private BusModeSetting _mode = BusModeSetting.Normal;

    /// <inheritdoc/>
    public override string ResourceType => nameof(Bus);

    /// <summary>
    /// Returns a value indicating if this is a virtual bus
    /// </summary>
    public virtual bool IsVirtual { get; internal set; }

    [VoiceMeeterParameter(nameof(_mono), "Mono", ParamType.Custom)]
    public BusMonoSetting Mono
    {
        get => this._mono;
        set => this.SetProperty(ref this._mono, value);
    }

    [VoiceMeeterParameter(nameof(_mute), "Mute", ParamType.Bool)]
    public bool Mute
    {
        get => this._mute;
        set => this.SetProperty(ref this._mute, value);
    }

    [VoiceMeeterParameter(nameof(_isEqEnabled), "EQ.on", ParamType.Bool)]
    public bool IsEqEnabled
    {
        get => this._isEqEnabled;
        set => this.SetProperty(ref this._isEqEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating which EQ memory slot is in use
    /// </summary>
    /// <remarks>A <c>false</c> value here means that EQ A is enabled (default), this will NOT enable the EQ if it's off, only select the slot</remarks>
    [VoiceMeeterParameter(nameof(_isEqBEnabled), "EQ.AB", ParamType.Bool)]
    public bool IsEqBEnabled
    {
        get => this._isEqBEnabled;
        set => this.SetProperty(ref this._isEqBEnabled, value);
    }

    [Range(IVoiceMeeterResource.MinGain, IVoiceMeeterResource.MaxGain)]
    [VoiceMeeterParameter(nameof(_gain),"Gain", ParamType.Float)]
    public float Gain
    {
        get => this._gain;
        set => this.SetProperty(ref this._gain, value);
    }

    [VoiceMeeterParameter(nameof(_deviceName), "device.name", ParamType.String, ParamMode = ParamMode.ReadOnly)]
    public string DeviceName
    {
        get => this._deviceName;
        internal set => this.SetProperty(ref this._deviceName, value);
    }

    [VoiceMeeterParameter(nameof(_sampleRate), "device.sr", ParamType.Float, ParamMode = ParamMode.ReadOnly)]
    public float SampleRate
    {
        get => this._sampleRate;
        internal set => this.SetProperty(ref this._sampleRate, value);
    }

    [VoiceMeeterParameter(nameof(_deviceName), "device.wdm", ParamType.String, ParamMode = ParamMode.WriteOnly)]
    public string WdmDevice
    {
        internal get => this._deviceName;
        set => this.SetProperty(ref this._deviceName, value);
    }

    [VoiceMeeterParameter(nameof(_fadeBy), "FadeBy", ParamType.Custom, ParamMode = ParamMode.WriteOnly)]
    public FadeSetting FadeBy
    {
        internal get => this._fadeBy;
        set => this.SetProperty(ref this._fadeBy, value);
    }
    
    [VoiceMeeterParameter(nameof(_fadeTo), "FadeTo", ParamType.Custom, ParamMode = ParamMode.WriteOnly)]
    public FadeSetting FadeTo
    {
        internal get => this._fadeTo;
        set => this.SetProperty(ref this._fadeTo, value);
    }

    [VoiceMeeterParameter(nameof(_mode), "mode", ParamType.CustomEnum)]
    public BusModeSetting Mode
    {
        get => this._mode;
        set => this.SetProperty(ref this._mode, value);
    }
    
    public Eq Eq { get; private set; }

    internal Bus(ChangeTracker changeTracker, VoiceMeeterType voiceMeeterType, int index) : base(changeTracker, voiceMeeterType, index)
    {
        this.Eq = new Eq(this.ChangeTracker, this);
    }
    
    internal Bus Init()
    {
        return this;
    }
}