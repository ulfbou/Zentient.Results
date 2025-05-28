using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>Mock extensions for <see cref="IServiceCollection"/> to facilitate testing with dependency injection.</summary>
    internal static class ServiceHelperExtensions
    {
        /// <summary>Adds a mock service to the <see cref="IServiceCollection"/>.</summary>
        /// <typeparam name="T">The type of the service to mock.</typeparam>
        /// <param name="services">The service collection to add the mock to.</param>
        /// <param name="mock">The mock instance to add.</param>
        /// <param name="serviceLifetime">The lifetime of the service (default is <see cref="ServiceLifetime.Singleton"/>).</param>
        public static IServiceCollection AddMock<T>(this IServiceCollection services, Mock<T> mock, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
            where T : class
        {
            services.Add(new ServiceDescriptor(typeof(T), mock.Object, serviceLifetime));
            return services;
        }
    }
}
