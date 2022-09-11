using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using VoiceMeeter.NET.Enums;

namespace VoiceMeeter.NET;

internal static class NativeMethods
{
    private const string BaseVoiceMeeterPath = @"C:\Program Files (x86)\VB\Voicemeeter";
    private const string RemoteLibraryName = "VoicemeeterRemote";

    static NativeMethods()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, Resolver);
    }

    private static IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != RemoteLibraryName) return IntPtr.Zero;

        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => NativeLibrary.Load($"{Path.Combine(BaseVoiceMeeterPath, RemoteLibraryName)}64.dll"),
            Architecture.X86 => NativeLibrary.Load($"{Path.Combine(BaseVoiceMeeterPath, RemoteLibraryName)}.dll"),
            _ => IntPtr.Zero
        };
    }

    #region Login

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_Login")]
    internal static extern LoginResponse Login();

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_Logout")]
    internal static extern long Logout();

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_RunVoicemeeter")]
    internal static extern long RunVoiceMeeter(long vType);

    #endregion

    #region General Information

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_GetVoicemeeterType")]
    internal static extern void GetVoiceMeeterType(out long pType);

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_GetVoicemeeterVersion")]
    internal static extern void GetVoiceMeeterVersion(out long pVersion);

    #endregion

    #region Get Parameters

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_IsParametersDirty")]
    internal static extern long IsParametersDirty();

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_GetParameterFloat")]
    internal static extern long GetParameter([MarshalAs(UnmanagedType.LPStr)] string paramName, out float value);

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode,
        EntryPoint = "VBVMR_GetParameterStringW")]
    internal static extern long GetParameter([MarshalAs(UnmanagedType.LPStr)] string paramName,
        char[] value);

    #endregion

    #region Get Levels

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_GetLevel")]
    internal static extern long GetLevel(long levelType, long channelNumber, out float value0);

    #endregion

    #region Set Parameters

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_SetParameterFloat")]
    internal static extern long SetParameter([MarshalAs(UnmanagedType.LPStr)] string paramName, float value);
    
    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_SetParameterStringW")]
    internal static extern long SetParameter([MarshalAs(UnmanagedType.LPStr)] string paramName, [MarshalAs(UnmanagedType.LPWStr)] string value);
    
    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_SetParametersW")]
    internal static extern long SetParameters([MarshalAs(UnmanagedType.LPWStr)] string paramScript);

    #endregion

    #region Devices Enumeration

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_Output_GetDeviceNumber")]
    internal static extern long GetOutputDeviceNumber();

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_Output_GetDeviceDescW")]
    internal static extern long GetOutputDeviceDescription(long deviceIndex, out long deviceType, char[] deviceName, char[] hardwareId);
    
    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_Input_GetDeviceNumber")]
    internal static extern long GetInputDeviceNumber();

    [DllImport(RemoteLibraryName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto,
        EntryPoint = "VBVMR_Input_GetDeviceDescW")]
    internal static extern long GetInputDeviceDescription(long deviceIndex, out long deviceType, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder deviceName, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder hardwareId);
    
    #endregion
}