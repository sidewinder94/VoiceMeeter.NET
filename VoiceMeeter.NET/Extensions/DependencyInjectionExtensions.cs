﻿using Microsoft.Extensions.DependencyInjection;

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
        serviceCollection.AddSingleton(_ => VoiceMeeterClient.Create());
        
        return serviceCollection;
    }
}