using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VoiceMeeter.NET.Extensions;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Add an instance of <see cref="IVoiceMeeterClient"/> to a <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add the <see cref="IVoiceMeeterClient"/> instance to</param>
    /// <returns>The same <see cref="IServiceCollection"/> to allow calls to be chained</returns>
    public static IServiceCollection AddVoiceMeeterClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(provider => VoiceMeeterClient.Create(provider.GetService<ILoggerFactory>()));
        
        return serviceCollection;
    }
}