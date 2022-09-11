namespace VoiceMeeter.NET.Enums;

/// <summary>
/// Represents the possible login states
/// </summary>
public enum LoginResponse
{
    AlreadyLoggedIn = -2,
    NoClient = -1,
    Ok = 0,
    VoiceMeeterNotRunning = 1,
    LoggedOff
}