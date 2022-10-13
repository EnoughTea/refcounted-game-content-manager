using System;
using System.Collections.Generic;

namespace RcContentManager
{
    /// <summary>
    ///     Simple implementation of <see cref="IServiceProvider" /> to use as a service provider
    ///     for a <see cref="IContentManager" />.
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <inheritdoc />
        public object? GetService(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return _services.TryGetValue(type, out object service) ? service : null;
        }

        /// <summary>Gets the service object of the specified type.</summary>
        /// <typeparam name="T">Type of the service object.</typeparam>
        /// <returns>
        ///     A service object of type <see cref="T" /> or <code>null</code> if
        ///     there is no service object of type <see cref="T" />.
        /// </returns>
        public T? GetService<T>() where T : class => GetService(typeof(T)) as T;

        /// <summary>Adds the given service to the container.</summary>
        /// <param name="type">Service type.</param>
        /// <param name="provider">Service provider instance.</param>
        /// <exception cref="ArgumentNullException">
        ///     type
        ///     or
        ///     provider
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Explicit provider type does not match service instance type
        /// </exception>
        public void AddService(Type type, object provider)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (!type.IsInstanceOfType(provider))
                throw new ArgumentException(
                    $"Explicit provider type {type.Name} does not match service instance type {provider.GetType().Name}");

            _services.Add(type, provider);
        }

        /// <summary>Adds the given service to the container.</summary>
        /// <param name="provider">Service provider instance.</param>
        /// <exception cref="ArgumentNullException">
        ///     type
        ///     or
        ///     provider
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Explicit provider type does not match service instance type
        /// </exception>
        public void AddService<T>(T provider) => AddService(typeof(T), provider!);

        /// <summary>Removes the service with the given type.</summary>
        /// <param name="type">Type of the service to remove.</param>
        /// <returns>true if service was found and removed; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public bool RemoveService(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return _services.Remove(type);
        }

        /// <summary>Removes the service with the given type.</summary>
        /// <typeparam name="T">Type of the service to remove.</typeparam>
        /// <returns>true if service was found and removed; false otherwise.</returns>
        public bool RemoveService<T>() => RemoveService(typeof(T));
    }
}