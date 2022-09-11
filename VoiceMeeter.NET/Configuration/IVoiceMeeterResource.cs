using System.ComponentModel;
using System.Reactive;

namespace VoiceMeeter.NET.Configuration;

public interface IVoiceMeeterResource: INotifyPropertyChanged
{
    public const float MinGain = -60.0f;
    public const float MaxGain = 12.0f;
    
    public int Index { get; }
    public string? Name { get; set; }
    IObservable<EventPattern<PropertyChangedEventArgs>> PropertyChangedObservable { get; }
}