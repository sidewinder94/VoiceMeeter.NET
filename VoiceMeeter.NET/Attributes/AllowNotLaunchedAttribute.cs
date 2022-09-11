namespace VoiceMeeter.NET.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class AllowNotLaunchedAttribute : Attribute
{
    public bool IgnoreIfLoggedOff { get; set; } 
}