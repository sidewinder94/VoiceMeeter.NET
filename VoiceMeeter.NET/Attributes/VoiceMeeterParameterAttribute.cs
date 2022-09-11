using System.Runtime.InteropServices;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class VoiceMeeterParameterAttribute : Attribute
{
    internal string StoreName { get; }
    public string Name { get; }
    public ParamType ParamType { get; }
    public ParamMode ParamMode { get; set; } = ParamMode.ReadWrite;
    public VoiceMeeterType[] UsableOn { get; }

    public VoiceMeeterParameterAttribute(string storeName, string name, ParamType paramType, [Optional] params VoiceMeeterType[] usableOn)
    {
        this.Name = name;
        this.ParamType = paramType;
        this.StoreName = storeName;
        this.UsableOn = usableOn.IsEmpty()
            ? new[]
            {
                VoiceMeeterType.VoiceMeeter, VoiceMeeterType.VoiceMeeterBanana, VoiceMeeterType.VoiceMeeterPotato
            }
            : usableOn;
    }
}