using JetBrains.Annotations;
using VoiceMeeter.NET.Attributes;
using VoiceMeeter.NET.Enums;
using VoiceMeeter.NET.Exceptions;
using VoiceMeeter.NET.Structs;

namespace VoiceMeeter.NET;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public interface IVoiceMeeterClient
{
    /// <summary>
    /// Returns the connection Status of the <see cref="IVoiceMeeterClient"/>
    /// </summary>
    LoginResponse Status { get; }
    
    /// <summary>
    /// Connect to the VoiceMeeter Remote API
    /// </summary>
    /// <returns>A <see cref="LoginResponse"/> describing the state of the connection</returns>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    LoginResponse Login();
    
    /// <summary>
    /// Disconnect from the VoiceMeeter Remote API
    /// </summary>
    /// <permission cref="AllowNotLaunchedAttribute">Can be used even if the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></permission>
    /// <returns><c>true</c> on success</returns>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/>, <see cref="LoginResponse.VoiceMeeterNotRunning"/> or <see cref="LoginResponse.LoggedOff"/></exception>
    /// <remarks>Can be called multiple times it will only effectively call VoiceMeeter if the <see cref="Status"/> is not <see cref="LoginResponse.LoggedOff"/></remarks>
    bool Logout();
    
    /// <summary>
    /// Request to start VoiceMeeter
    /// </summary>
    /// <param name="voiceMeeterType">The <see cref="VoiceMeeterType"/> to run</param>
    /// <permission cref="AllowNotLaunchedAttribute">Can be used even if the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></permission>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/> or <see cref="LoginResponse.VoiceMeeterNotRunning"/></exception>
    /// <exception cref="VoiceMeeterException">If VoiceMeeter is not installed</exception>
    /// <exception cref="ArgumentOutOfRangeException">If an unknown <see cref="VoiceMeeterType"/> is used</exception>
    void RunVoiceMeeter(VoiceMeeterType voiceMeeterType);
    
    /// <summary>
    /// Discover the <see cref="VoiceMeeterType"/> of the running instance of VoiceMeeter
    /// </summary>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <returns>The <see cref="VoiceMeeterType"/> of the running VoiceMeeter instance</returns>
    VoiceMeeterType GetVoiceMeeterType();
    
    /// <summary>
    /// Obtains the version of the running VoiceMeeter instance
    /// </summary>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <returns>A <see cref="Version"/> object representing the running VoiceMeeter version</returns>
    Version GetVoiceMeeterVersion();
    
    /// <summary>
    /// Detects if VoiceMeeter data was updated
    /// </summary>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <returns><c>true</c> if new data is available</returns>
    bool IsDirty();
    
    /// <summary>
    /// Obtain the <see cref="float"/> value of a given parameter
    /// </summary>
    /// <param name="paramName">The name of the parameter to get the value for</param>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <see cref="paramName"/> is unknown by VoiceMeeter</exception>
    /// <exception cref="VoiceMeeterException">In cas of an unexpected error (e.g. structure mismatch)</exception>
    /// <returns>The current <see cref="float"/> value of the parameter</returns>
    /// <remarks>A first call to <see cref="IsDirty"/> might be necessary to obtain values after <see cref="Login"/></remarks>
    float GetFloatParameter(string paramName);
    
    /// <summary>
    /// Obtain the <see cref="string"/> value of a given parameter
    /// </summary>
    /// <param name="paramName">The name of the parameter to get the value for</param>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <see cref="paramName"/> is unknown by VoiceMeeter</exception>
    /// <exception cref="VoiceMeeterException">In cas of an unexpected error (e.g. structure mismatch)</exception>
    /// <returns>The current <see cref="string"/> value of the parameter</returns>
    /// <remarks>A first call to <see cref="IsDirty"/> might be necessary to obtain values after <see cref="Login"/></remarks>
    string GetStringParameter(string paramName);
    
    /// <summary>
    /// Create a <see cref="VoiceMeeterConfiguration"/> object that will hold and can refresh configuration values from VoiceMeeter,
    /// also able to push configuration changes to VoiceMeeter
    /// </summary>
    /// <param name="refreshDelay">The refresh delay, if <c>null</c> only pulls once to populate all fields</param>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">If the <see cref="paramName"/> is unknown by VoiceMeeter</exception>
    /// <returns>A <see cref="VoiceMeeterConfiguration"/> object</returns>
    /// <remarks>The <see cref="VoiceMeeterConfiguration"/> might not have immediately populated fields by the time it's returned</remarks>
    VoiceMeeterConfiguration GetConfiguration(TimeSpan? refreshDelay = null);
    
    /// <summary>
    /// Reads the number of output devices VoiceMeeter detected
    /// </summary>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <returns>The number of output devices</returns>
    long GetOutputDeviceCount();
    
    /// <summary>
    /// Obtains details about a given output device
    /// </summary>
    /// <param name="index">The VoiceMeeter index of the device to get details about</param>
    /// <exception cref="DllNotFoundException">In case the VoiceMeeterRemote.dll / VoiceMeeterRemote64.dll files are not found</exception>
    /// <exception cref="VoiceMeeterNotLoggedException">In case the <see cref="Status"/> is not <see cref="LoginResponse.Ok"/></exception>
    /// <exception cref="VoiceMeeterException">Unknown error (possibly invalid index?)</exception>
    /// <returns>The details about an output device</returns>
    /// <remarks>The <see cref="VoiceMeeterDevice.HardwareId"/> property, seems be empty on all non ASIO devices</remarks>
    VoiceMeeterDevice GetOutputDevice(long index);
}