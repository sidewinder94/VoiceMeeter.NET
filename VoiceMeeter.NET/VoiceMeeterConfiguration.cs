using JetBrains.Annotations;
using VoiceMeeter.NET.Configuration;
using VoiceMeeter.NET.Enums;
using VoiceMeeter.NET.Extensions;

namespace VoiceMeeter.NET;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class VoiceMeeterConfiguration : IDisposable
{
    private readonly VoiceMeeterType _voiceMeeterType;
    private CancellationTokenSource _cts = new();

    internal CancellationToken RefreshCancellationToken => this._cts.Token;
    
    /// <summary>
    /// Returns the <see cref="IVoiceMeeterClient"/> that is used by this <see cref="VoiceMeeterConfiguration"/> object
    /// </summary>
    public IVoiceMeeterClient Client { get; }
    
    /// <summary>
    /// Returns the <see cref="ChangeTracker"/> that is used to poll / push values from / to VoiceMeeter
    /// </summary>
    public ChangeTracker ChangeTracker { get; }
    
    /// <summary>
    /// Returns a <see cref="Dictionary{TKey,TValue}"/> that holds all the available <see cref="Strip"/>
    /// </summary>
    /// <remarks>The <see cref="Dictionary{TKey,TValue}.Keys"/> are either the name defined in VoiceMeeter or <c>Strip - index</c></remarks>
    /// <remarks><br/>This holds only the <see cref="Strip"/>s that exist in the current VoiceMeeter instance</remarks>
    public Dictionary<string, Strip> Strips { get; } = new();
    
    /// <summary>
    /// Returns a <see cref="Dictionary{TKey,TValue}"/> that holds all the available <see cref="Bus"/>
    /// </summary>
    /// <remarks>The <see cref="Dictionary{TKey,TValue}.Keys"/> are the Bus index from VoiceMeeter (e.g. A1)</remarks>
    /// <remarks><br/>This holds only the <see cref="Bus"/>es that exist in the current VoiceMeeter instance</remarks>
    public Dictionary<string, Bus> Buses { get; } = new();
    
    internal VoiceMeeterConfiguration(VoiceMeeterClient client, TimeSpan? refreshFrequency,
        VoiceMeeterType voiceMeeterType)
    {
        this._voiceMeeterType = voiceMeeterType;
        this.Client = client;
        this.ChangeTracker = new ChangeTracker(client, this, refreshFrequency);
    }

    internal VoiceMeeterConfiguration Init()
    {
        int stripCount;
        int virtualStripCount;
        int busCount;
        int virtualBusCount;

        switch (this._voiceMeeterType)
        {
            case VoiceMeeterType.VoiceMeeter:
                stripCount = 3;
                virtualStripCount = 1;
                busCount = 2;
                virtualBusCount = 1;
                break;
            case VoiceMeeterType.VoiceMeeterBanana:
                stripCount = 5;
                virtualStripCount = 2;
                busCount = 5;
                virtualBusCount = 2;
                break;
            case VoiceMeeterType.VoiceMeeterPotato:
                stripCount = 8;
                virtualStripCount = 3;
                busCount = 8;
                virtualBusCount = 4;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        for (var s = 0; s < stripCount; s++)
        {
            var newStrip = new Strip(this.ChangeTracker, this._voiceMeeterType, index: s);
        
            string stripName = this.Client.GetStringParameter(newStrip.GetPropertyParamName(ns => ns.Name));

            if (string.IsNullOrWhiteSpace(stripName))
            {
                stripName = newStrip.ResourceType;
            }

            if (this.Strips.ContainsKey(stripName))
            {
                stripName += $" - {s}";
            }

            // VAIO / AUX / VAIO3
            if (s >= stripCount - virtualStripCount)
            {
                newStrip.VirtualDeviceName = (s - (stripCount - virtualStripCount)) switch
                {
                    0 => "VoiceMeeter Input (VB-Audio VoiceMeeter VAIO)",
                    1 => "VoiceMeeter Aux Input (VB-Audio VoiceMeeter AUX VAIO)",
                    2 => "VoiceMeeter VAIO3 Input (VB-Audio VoiceMeeter VAIO3)",
                    _ => throw new IndexOutOfRangeException()
                };
            }
            

            this.Strips[stripName] = newStrip.Init();
        }

        var realBusIndex = 1;
        var virtualBusIndex = 1;

        for (var b = 0; b < busCount; b++)
        {
            var newBus = new Bus(this.ChangeTracker, this._voiceMeeterType, index: b);

            string busName = newBus.ResourceType;

            if (busCount - (realBusIndex + virtualBusCount) >= 0)
            {
                busName = $"A{realBusIndex}";
                realBusIndex += 1;
                newBus.IsVirtual = false;
            } 
            else if (virtualBusIndex <= virtualBusCount)
            {
                busName = $"B{virtualBusIndex}";
                virtualBusIndex += 1;
                newBus.IsVirtual = true;
            }

            if (this.Strips.ContainsKey(busName))
            {
                busName += $" - {b}";
            }

            this.Buses[busName] = newBus.Init();
        }

        return this;
    }
    
    /// <summary>
    /// Stop the polling (if any), and release unmanaged resources
    /// </summary>
    /// <remarks>Does <b>NOT</b> dispose or <see cref="IVoiceMeeterClient.Logout"/> the <see cref="IVoiceMeeterClient"/></remarks>
    public void Dispose()
    {
        this._cts.Cancel();
        this._cts.Dispose();
    }
}