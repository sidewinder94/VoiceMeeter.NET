namespace VoiceMeeter.NET.Exceptions;

/// <summary>
/// The exception thrown when a general VoiceMeeter related error occured
/// </summary>
public class VoiceMeeterException : Exception
{
    public VoiceMeeterException() : base()
    {}
    
    public VoiceMeeterException(string message) : base(message)
    {
        
    }
}