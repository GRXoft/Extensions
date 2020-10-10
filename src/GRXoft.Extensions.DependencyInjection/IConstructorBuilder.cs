using System;

namespace GRXoft.Extensions.DependencyInjection
{
    public interface IConstructorBuilder<out TService>
    {
        /// <summary>
        /// Creates a delegate capable of instantiating <typeparamref name="TService"/>
        /// using given <see cref="IServiceProvider"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// 
        /// </exception>
        Func<IServiceProvider, TService> Build();

        /// <summary>
        /// Configures the parameter with given <paramref name="name"/> or type to be resolved
        /// using given <paramref name="resolver"/>.
        /// </summary>
        /// <typeparam name="T">Parameter type.</typeparam>
        /// <param name="name">
        /// Parameter name. If <see langword="null"/>, the parameter will be
        /// resolved by matching against <typeparamref name="T"/>.
        /// </param>
        /// <param name="resolver">
        /// A delegate that would produce the parameter value using given <see cref="IServiceProvider"/>.
        /// </param>
        /// <param name="overwrite">
        /// Value indicating whether existing parameter configuration should be overwritten.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is an empty string or contains only whitespace.
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