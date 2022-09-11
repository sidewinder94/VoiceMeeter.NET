using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using VoiceMeeter.NET.Attributes;
using VoiceMeeter.NET.Enums;
using VoiceMeeter.NET.Exceptions;
using VoiceMeeter.NET.Extensions;
using VoiceMeeter.NET.Structs;

namespace VoiceMeeter.NET;

public class VoiceMeeterClient : IVoiceMeeterClient, IDisposable
{
    [UsedImplicitly] 
    public LoginResponse Status { get; private set; } = LoginResponse.LoggedOff;

    private readonly object _lockObj = new();

    private VoiceMeeterClient()
    {
    }

    /// <summary>
    /// Creates a client able to interact with VoiceMeeter remote API
    /// </summary>
    /// <seealso cref="DependencyInjectionExtensions.AddVoiceMeeterClient"/>
    /// <returns>An instance of <see cref="IVoiceMeeterClient"/></returns>
    public static IVoiceMeeterClient Create()
    {
        var proxyGenerator = new ProxyGenerator();
        var client = new VoiceMeeterClient();
        var interceptor = new ClientInterceptor(client);
        return proxyGenerator.CreateInterfaceProxyWithTargetInterface<IVoiceMeeterClient>(client, interceptor);
    }

    /// <inheritdoc/>
    public LoginResponse Login()
    {
        this.Status = NativeMethods.Login();

        return this.Status;
    }

    /// <inheritdoc/>
    [AllowNotLaunched(IgnoreIfLoggedOff = true)]
    public bool Logout()
    {
        long result = NativeMethods.Logout();

        if (result != 0) return false;

        this.Status = LoginResponse.LoggedOff;
        return true;
    }

    /// <inheritdoc/>
    [AllowNotLaunched]
    public void RunVoiceMeeter(VoiceMeeterType voiceMeeterType)
    {
        long result = NativeMethods.RunVoiceMeeter((long)voiceMeeterType);

        switch (result)
        {
            case -1:
                throw new VoiceMeeterException("VoiceMeeter is not installed");
            case -2:
                throw new ArgumentOutOfRangeException(nameof(voiceMeeterType));
        }
    }
    
    /// <inheritdoc/>
    public  VoiceMeeterConfiguration GetConfiguration(TimeSpan? refreshDelay = null)
    {
        return new VoiceMeeterConfiguration(this, refreshDelay, this.GetVoiceMeeterType()).Init();
    }

    /// <inheritdoc/>
    public VoiceMeeterType GetVoiceMeeterType()
    {
        NativeMethods.GetVoiceMeeterType(out long result);

        return (VoiceMeeterType)result;
    }

    /// <inheritdoc/>
    public Version GetVoiceMeeterVersion()
    {
        NativeMethods.GetVoiceMeeterVersion(out long version);

        return new Version(
            (int)((version & 0xFF000000) >> 24),
            (int)((version & 0x00FF0000) >> 16),
            (int)((version & 0x0000FF00) >> 8),
            (int)version & 0x000000FF);
    }

    /// <inheritdoc/>
    public bool IsDirty()
    {
        lock (this._lockObj)
        {
            bool result = NativeMethods.IsParametersDirty() == 1;

            return result;
        }
    }

    /// <inheritdoc/>
    public float GetFloatParameter(string paramName)
    {
        lock (this._lockObj)
        {
            long status = NativeMethods.GetParameter(paramName, out float result);

            AssertGetParamResult(status, paramName);

            return result;
        }
    }

    /// <inheritdoc/>
    public string GetStringParameter(string paramName)
    {
        lock (this._lockObj)
        {
            char[] buffer = ArrayPool<char>.Shared.Rent(512 + 1);

            try
            {
                long status = NativeMethods.GetParameter(paramName, buffer);

                AssertGetParamResult(status, paramName);

                return buffer.GetStringFromNullTerminatedCharArray();
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer, true);
            }
        }
    }

    /// <inheritdoc/>
    public long GetOutputDeviceCount()
    {
        return NativeMethods.GetOutputDeviceNumber();
    }

    /// <inheritdoc/>
    public VoiceMeeterDevice GetOutputDevice(long index)
    {
        char[] deviceNameBuffer = ArrayPool<char>.Shared.Rent(512 + 1);
        char[] deviceHardwareIdBuffer = ArrayPool<char>.Shared.Rent(512 + 1);

        try
        {
            long status = NativeMethods.GetOutputDeviceDescription(index, out long type, deviceNameBuffer, deviceHardwareIdBuffer);

            if (status != 0)
            {
                throw new VoiceMeeterException("Unknown Error");
            }
            
            return new VoiceMeeterDevice()
            {
                DeviceType = (DeviceType)type,
                Name = deviceNameBuffer.GetStringFromNullTerminatedCharArray(),
                HardwareId = deviceHardwareIdBuffer.GetStringFromNullTerminatedCharArray()
            };
        }
        finally
        {
            ArrayPool<char>.Shared.Return(deviceNameBuffer, true);
            ArrayPool<char>.Shared.Return(deviceHardwareIdBuffer, true);
        }
    }

    [SuppressMessage("Performance", 
        "CA1822:Mark members as static", 
        Justification = "We need the logged in check that is done through proxy, check that access an instance field.")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    internal void SetParameter(string paramName, string value)
    {
        long status = NativeMethods.SetParameter(paramName, value);

        AssertSetParamResult(status, paramName);
    }

    [SuppressMessage("Performance", 
        "CA1822:Mark members as static", 
        Justification = "We need the logged in check that is done through proxy, check that access an instance field.")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    internal void SetParameter(string paramName, float value)
    {
        long status = NativeMethods.SetParameter(paramName, value);

        AssertSetParamResult(status, paramName);
    }

    [SuppressMessage("Performance", 
        "CA1822:Mark members as static", 
        Justification = "We need the logged in check that is done through proxy, check that access an instance field.")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    internal void SetParameters(string script)
    {
        long status = NativeMethods.SetParameters(script);

        switch (status)
        {
            case 0: break;
            case > 0: throw new VoiceMeeterScriptException($"Script error on line {status}", script);
            case -2: throw new VoiceMeeterNotLoggedException();
            default: throw new VoiceMeeterException("Unknown Error");
        }
    }

    private static void AssertSetParamResult(long result, string paramName)
    {
        switch (result)
        {
            case 0: return;
            case -1: throw new VoiceMeeterException("Error");
            case -2: throw new VoiceMeeterNotLoggedException();
            case -3: throw new ArgumentOutOfRangeException(paramName);
            default: throw new VoiceMeeterException("Unknown Error");
        }
    }

    private static void AssertGetParamResult(long result, string paramName)
    {
        switch (result)
        {
            case 0: return;
            case -1: throw new VoiceMeeterException("Error");
            case -2: throw new VoiceMeeterNotLoggedException();
            case -3: throw new ArgumentOutOfRangeException(paramName);
            case -5: throw new VoiceMeeterException("Structure Mismatch");
            default: throw new VoiceMeeterException("Unknown Error");
        }
    }

    private class ClientInterceptor : IInterceptor
    {
        private IVoiceMeeterClient Client { get; }

        public ClientInterceptor(IVoiceMeeterClient client)
        {
            this.Client = client;
        }

        private void AssertLoggedIn(bool allowNotLaunched = false)
        {
            // Allows execution to continue if Status is ok
            if (this.Client.Status == LoginResponse.Ok ||
                // Or if OkButNotLaunched allowed, allow that status too
                (allowNotLaunched && this.Client.Status == LoginResponse.VoiceMeeterNotRunning)) return;

            // Else throw
            throw new VoiceMeeterNotLoggedException();
        }

        public void Intercept(IInvocation invocation)
        {
            // ReSharper disable once ArrangeThisQualifier // Cannot add this, there is no Login property in this subclass R# Bug ?
            if (invocation.Method.Name != nameof(Login))
            {
                var allowNotLaunched =
                    invocation.MethodInvocationTarget.GetCustomAttribute<AllowNotLaunchedAttribute>();

                // Allows for multiple log off calls without exceptions
                if (allowNotLaunched is { IgnoreIfLoggedOff: true } && this.Client.Status == LoginResponse.LoggedOff)
                {
                    invocation.ReturnValue = false;
                    return;
                }
                
                this.AssertLoggedIn(allowNotLaunched != null);
            }

            invocation.Proceed();
        }
    }

    private void ReleaseUnmanagedResources()
    {
        this.Logout();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    ~VoiceMeeterClient()
    {
        this.ReleaseUnmanagedResources();
    }
}