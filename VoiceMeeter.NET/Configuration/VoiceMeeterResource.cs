using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using VoiceMeeter.NET.Attributes;
using VoiceMeeter.NET.Configuration.Values;
using VoiceMeeter.NET.Enums;


namespace VoiceMeeter.NET.Configuration;

/// <summary>
/// Class providing the common base functionality for all VoiceMeeter Resources 
/// </summary>
/// <typeparam name="TResource">The type of the VoiceMeeter resource this base class is used for</typeparam>
public abstract class VoiceMeeterResource<TResource> : IVoiceMeeterResource
    where TResource : VoiceMeeterResource<TResource>
{
    private string? _name;
    private VoiceMeeterType VoiceMeeterType { get; }
    protected ChangeTracker ChangeTracker { get; }
    protected event PropertyChangedEventHandler? RemoteValueToUpdate;

    private Dictionary<string, (VoiceMeeterParameterAttribute Attribute, PropertyInfo Property, FieldInfo Field)>
        VoiceMeeterProperties { get; }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The string representation of this resource type
    /// </summary>
    public abstract string ResourceType { get; }

    /// <summary>
    /// The resource index, for unique resource, will still be set to <c>0</c>
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Returns an <see cref="IObservable{T}"/> pushing the values of the <see cref="PropertyChanged"/> event
    /// </summary>
    /// <remarks>Supports multicast</remarks>
    public IObservable<EventPattern<PropertyChangedEventArgs>> PropertyChangedObservable { get; }

    [VoiceMeeterParameter(nameof(_name), "Label", ParamType.String)]
    public string? Name
    {
        get => this._name;
        set => this.SetProperty(ref this._name, value);
    }

    protected VoiceMeeterResource(ChangeTracker changeTracker, VoiceMeeterType voiceMeeterType, int index)
    {
        this.Index = index;
        this.ChangeTracker = changeTracker;
        this.VoiceMeeterType = voiceMeeterType;

        this.VoiceMeeterProperties = typeof(TResource).GetProperties()
            .Select(prop =>
            {
                var attribute = prop.GetCustomAttribute<VoiceMeeterParameterAttribute>(inherit: true);
                return
                    (Name: attribute?.Name,
                        Attribute: attribute,
                        Property: prop);
            })
            .Where(prop => prop.Name != null && prop.Attribute != null)
            .Select(info =>
            {
                var field = this.GetType().GetField(info.Attribute!.StoreName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                return (Name: info.Name, Attribute: info.Attribute, Property: info.Property, Field: field);
            })
            .Where(info => info.Field != null)
            .ToDictionary(k => k.Name!, v => (Attribute: v.Attribute!, Property: v.Property, Field: v.Field!));

        this.ChangeTracker.RefreshEventObservable
            .Subscribe(
                this.OnUpdateTriggered,
                changeTracker.VoiceMeeterConfiguration.RefreshCancellationToken);

        Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => this.RemoteValueToUpdate += handler,
                handler => this.RemoteValueToUpdate -= handler)
            .Subscribe(this.OnValueToUpdate, changeTracker.VoiceMeeterConfiguration.RefreshCancellationToken);

        this.PropertyChangedObservable = Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                handler => this.PropertyChanged += handler,
                handler => this.PropertyChanged -= handler)
            .Publish()
            .RefCount();
    }


    /// <summary>
    /// Pushes changes to <see cref="ChangeTracker"/>
    /// </summary>
    /// <param name="eventPattern"></param>
    protected virtual void OnValueToUpdate(EventPattern<PropertyChangedEventArgs> eventPattern)
    {
        if (eventPattern.EventArgs.PropertyName == null) return;

        string? propertyName = eventPattern.EventArgs.PropertyName;

        if (string.IsNullOrWhiteSpace(propertyName) || !this.VoiceMeeterProperties.ContainsKey(propertyName)) return;

        var (attribute, property, _) = this.VoiceMeeterProperties[propertyName];

        propertyName = this.GetFullParamName(propertyName);

        object? untypedValue = property.GetValue(this);

        if (untypedValue == null) throw new ArgumentNullException(propertyName);

        if (!attribute.UsableOn.Contains(this.VoiceMeeterType)) throw new InvalidOperationException();

        switch (attribute.ParamType)
        {
            case ParamType.Float:
                this.ChangeTracker.SaveValue(propertyName, (float)untypedValue);
                break;
            case ParamType.String:
                this.ChangeTracker.SaveValue(propertyName, (string)untypedValue);
                break;
            case ParamType.Bool:
                this.ChangeTracker.SaveValue(propertyName, (bool)untypedValue);
                break;
            case ParamType.Custom:
            case ParamType.CustomEnum:
                this.ChangeTracker.SaveValue(propertyName, (ICustomConfigurationSetting)untypedValue);
                break;
            default:
                throw new ArgumentOutOfRangeException(propertyName);
        }
    }


    /// <summary>
    /// Triggered by the <see cref="ChangeTracker"/> interval observable, will pull values from <see cref="VoiceMeeterClient"/> to update values
    /// </summary>
    /// <param name="isDirty">If VoiceMeeter has updates available</param>
    /// <exception cref="ArgumentOutOfRangeException">If the <see cref="VoiceMeeterParameterAttribute.ParamType"/> is not a known value</exception>
    [SuppressMessage("Usage", "CA2208",
        Justification = "We throw for another property, so that can't be our own argument")]
    protected virtual void OnUpdateTriggered(bool isDirty)
    {
        if (!isDirty) return;
        
#if DEBUG
        var stopWatch = new System.Diagnostics.Stopwatch();       
        stopWatch.Start();
#endif
        
        foreach ((string name, var (attribute, property, field)) in this.VoiceMeeterProperties)
        {
            if (attribute.ParamMode == ParamMode.WriteOnly) continue;

            switch (attribute.ParamType)
            {
                case ParamType.Integer:
                    int valueInt = Convert.ToInt32(this.ChangeTracker.Client.GetFloatParameter(this.GetFullParamName(name)));
                    field.SetValue(this, valueInt);
                    break;
                case ParamType.Float:
                    float valueFloat = this.ChangeTracker.Client.GetFloatParameter(this.GetFullParamName(name));
                    field.SetValue(this, valueFloat);
                    break;
                case ParamType.String:
                    string valueString = this.ChangeTracker.Client.GetStringParameter(this.GetFullParamName(name));
                    field.SetValue(this, valueString);
                    break;
                case ParamType.Bool:
                    bool valueBool = this.ChangeTracker.Client.GetFloatParameter(this.GetFullParamName(name)) != 0;
                    field.SetValue(this, valueBool);
                    break;
                case ParamType.Custom:
                case ParamType.CustomEnum:
                    ParamType? customParamType = ((ICustomConfigurationSetting?)field.GetValue(this))?.ValueType;
                    var valueCustom =
                        this.ChangeTracker.GetCustomParameter(field, customParamType, this.GetFullParamName(name));
                    field.SetValue(this, valueCustom);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attribute));
            }

            this.PropertyChanged?.Invoke(this.ChangeTracker, new PropertyChangedEventArgs(property.Name));
        }
        
#if DEBUG
        stopWatch.Stop();
        //System.Diagnostics.Trace.TraceInformation("Pull duration was {0} / {1} ms", stopWatch.Elapsed, stopWatch.ElapsedMilliseconds);
#endif
    }

    internal virtual string GetFullParamName(string paramName)
    {
        return $"{this.ResourceType}[{this.Index}].{paramName}";
    }


    [NotifyPropertyChangedInvocator]
    protected virtual void SetProperty<T>(ref T store, T value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) throw new ArgumentNullException(propertyName);

        if (Equals(store, value))
        {
            return;
        }

        var property = this.GetType().GetProperty(propertyName);

        var validRange = property?.GetCustomAttribute<RangeAttribute>();
        var parameterAttribute = property?.GetCustomAttribute<VoiceMeeterParameterAttribute>();

        if (validRange != null && !validRange.IsValid(value))
        {
            throw new ArgumentOutOfRangeException(propertyName, value,
                $"Value should be between {validRange.Minimum} and {validRange.Maximum}");
        }

        store = value;

        if (parameterAttribute is { ParamMode: ParamMode.ReadOnly })
        {
            this.PropertyChanged?.Invoke(this.ChangeTracker, new PropertyChangedEventArgs(propertyName));
            return;
        }

        this.RemoteValueToUpdate?.Invoke(this, new PropertyChangedEventArgs(parameterAttribute?.Name ?? propertyName));
    }
}