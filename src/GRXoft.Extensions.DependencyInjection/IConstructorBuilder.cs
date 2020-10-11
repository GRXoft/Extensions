using System;

namespace GRXoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides means to setup construction of instances of <typeparamref name="TService"/>.
    /// <para>
    /// By default, all parameters should be resolved by calling <see cref="IServiceProvider.GetService(Type)"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TService">Constructed service type.</typeparam>
    public interface IConstructorBuilder<out TService>
    {
        /// <summary>
        /// Creates a delegate capable of instantiating <typeparamref name="TService"/>
        /// using an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a construction delegate could not be created. More details
        /// should be provided in <see cref="Exception.InnerException"/>.
        /// </exception>
        Func<IServiceProvider, TService> Build();

        /// <summary>
        /// Configures a contructor parameter to be resolved using given <paramref name="resolver"/>.
        /// </summary>
        /// <typeparam name="T">Parameter type.</typeparam>
        /// <param name="name">
        /// Parameter name to match against.
        /// <para/>
        /// If <see langword="null"/>, the parameter will be matched against type <typeparamref name="T"/> instead.
        /// </param>
        /// <param name="resolver">
        /// A delegate that would produce parameter value given an <see cref="IServiceProvider"/>.
        /// <para/>
        /// If <see langword="null"/>, the builder should clear existing matching parameter resolver
        /// (if <paramref name="overwrite"/> is <see langword="true"/>) and fall back to default
        /// parameter value resolver (by default a call to <see cref="IServiceProvider.GetService(Type)"/>).
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
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="overwrite"/> is <see langword="false"/> and a matching parameter resolver was already configured.
        /// </exception>
        void ResolveParameter<T>(string name, Func<IServiceProvider, T> resolver, bool overwrite);
    }
}