using System;

namespace GRXoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides means to setup construction of an instance of <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">Constructed service type.</typeparam>
    public interface IConstructorBuilder<out TService>
    {
        /// <summary>
        /// Creates a delegate capable of instantiating <typeparamref name="TService"/>
        /// using given <see cref="IServiceProvider"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a construction delegate could not be created. More details
        /// should be provided in <see cref="Exception.InnerException"/>.
        /// </exception>
        Func<IServiceProvider, TService> Build();

        /// <summary>
        /// Configures the parameter with given <paramref name="name"/> or type to be resolved
        /// using given <paramref name="resolver"/>.
        /// </summary>
        /// <typeparam name="T">Parameter type.</typeparam>
        /// <param name="name">
        /// Parameter name. If <see langword="null"/>, the parameter will be
        /// resolved by matching against <typeparamref name="T"/> instead.
        /// </param>
        /// <param name="resolver">
        /// A delegate that would produce the parameter value using given <see cref="IServiceProvider"/>.
        /// </param>
        /// <param name="overwrite">
        /// Value indicating whether existing parameter configuration should be overwritten.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> was given, then the exception is thrown when:
        /// <list type="bullet">
        /// <item><paramref name="name"/> is an empty string or contains only whitespace</item>
        /// <item>Parameter with given <paramref name="name"/> was not found</item>
        /// <item>Parameter with given <paramref name="name"/> is not compatible with type <typeparamref name="T"/></item>
        /// </list>
        /// Otherwise the exception is thrown when:
        /// <list type="bullet">
        /// <item>Parameter of given type <typeparamref name="T"/> was not found</item>
        /// <item>There are multiple parameters matchin type <typeparamref name="T"/></item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="resolver"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="overwrite"/> is <see langword="false"/> and a matching
        /// parameter resolution is already configured.
        /// </exception>
        void ResolveParameter<T>(string name, Func<IServiceProvider, T> resolver, bool overwrite);
    }
}