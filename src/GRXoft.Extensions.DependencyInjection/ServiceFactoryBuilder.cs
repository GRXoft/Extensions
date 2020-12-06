using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GRXoft.Extensions.DependencyInjection
{
    public sealed class ServiceFactoryBuilder
    {
        private static readonly MethodInfo _method = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService));

        private readonly ConstructorInfo _constructor;
        private readonly IDictionary<string, Delegate> _parameterResolvers;
        private readonly IReadOnlyList<ParameterInfo> _parameters;

        public ServiceFactoryBuilder(Type type) : this(SelectConstructor(type))
        {
        }

        public ServiceFactoryBuilder(ConstructorInfo constructor)
        {
            _constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            _parameters = _constructor.GetParameters();

            foreach (var parameter in _parameters)
            {
                if (parameter.IsOut || parameter.ParameterType.IsByRef)
                    throw new Exception(); // TODO
            }

            _parameterResolvers = new Dictionary<string, Delegate>();
        }

        public Func<IServiceProvider, object> Build()
        {
            var serviceProvider = Expression.Parameter(typeof(IServiceProvider), "sp");

            var parameters = new Expression[_parameters.Count];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = _parameters[i];
                if (_parameterResolvers.TryGetValue(parameter.Name, out var resolver))
                    parameters[i] = BuildCustomParameterExpression(serviceProvider, parameter, resolver);
                else
                    parameters[i] = BuildDefaultParameterExpression(serviceProvider, parameter);
            }

            var ctor = Expression.New(_constructor, parameters);
            var lambda = Expression.Lambda<Func<IServiceProvider, object>>(ctor, serviceProvider);

            return lambda.Compile();
        }

        /// <summary>
        /// Configures parameter matched by <paramref name="type"/> to be resolved by given <paramref name="resolver"/>.
        /// </summary>
        /// <returns>This instance, for chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="type"/> or <paramref name="resolver"/> is null.
        /// </exception>
        /// <inheritdoc cref="Resolve(string, Type, Delegate, bool)"/>
        public ServiceFactoryBuilder Resolve(Type type, Func<IServiceProvider, object> resolver, bool overwrite = false)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (resolver is null)
                throw new ArgumentNullException(nameof(resolver));

            Resolve(null, type, resolver, overwrite);

            return this;
        }

        public ServiceFactoryBuilder Resolve(string name, Func<IServiceProvider, object> resolver, bool overwrite)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(); // TODO

            if (resolver is null)
                throw new ArgumentNullException(nameof(resolver));

            Resolve(name, null, resolver, overwrite);

            return this;
        }

        private static Expression BuildCustomParameterExpression(Expression serviceProvider, ParameterInfo parameter, Delegate resolver)
        {
            var expression = (Expression)Expression.Invoke(Expression.Constant(resolver), serviceProvider);

            if (!parameter.ParameterType.Equals(resolver.Method.ReturnType))
                expression = Expression.Convert(expression, parameter.ParameterType);

            return expression;
        }

        private static Expression BuildDefaultParameterExpression(ParameterExpression serviceProvider, ParameterInfo parameter)
        {
            var type = Expression.Constant(parameter.ParameterType);
            var expression = (Expression)Expression.Call(serviceProvider, _method, type);
            if (!parameter.ParameterType.Equals(_method.ReturnType))
                expression = Expression.Convert(expression, parameter.ParameterType);

            return expression;
        }

        private static ConstructorInfo SelectConstructor(Type type)
        {
            if (type.IsInterface || type.IsAbstract)
                throw new Exception(); // TODO

            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (ctors.Length == 0)
                ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            if (ctors.Length == 0)
                throw new Exception(); // TODO

            if (ctors.Length > 1)
                throw new Exception(); // TODO

            return ctors[0];
        }

        /// <param name="name">Name of the parameter to be matched.</param>
        /// <param name="type">Type of the parameter to be matched.</param>
        /// <exception cref="ArgumentException">
        /// Either none or multiple parameters match specified criteria.
        /// </exception>
        private ParameterInfo MatchParameter(string name, Type type)
        {
            Debug.Assert(name is string || type is Type);

            var matchingParameters = _parameters.AsEnumerable();

            if (name is string)
                matchingParameters = matchingParameters.Where(p => p.Name.Equals(name, StringComparison.Ordinal));

            if (type is Type)
                matchingParameters = matchingParameters.Where(p => p.ParameterType.Equals(type));

            var enumerator = matchingParameters.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                throw new ArgumentException(
                    "No parameters match specified criteria " +
                    $"(name={name}, type={type?.FullName})"
                );
            }

            var matchedParameter = enumerator.Current;

            if (enumerator.MoveNext())
            {
                throw new ArgumentException(
                    "Multiple parameters match specified criteria " +
                    $"(name={name}, type={type?.FullName})"
                );
            }

            return matchedParameter;
        }

        /// <param name="resolver">Deletage that resolves parameter value.</param>
        /// <param name="overwrite">Value indicating whether any pre-existing configuration should be overwritten.</param>
        /// <exception cref="InvalidOperationException">
        /// Matching parameter is already configured and <paramref name="overwrite"/> switch is set to false.
        /// </exception>
        /// <inheritdoc cref="MatchParameter"/>
        private void Resolve(string name, Type type, Delegate resolver, bool overwrite)
        {
            var matchedParameter = MatchParameter(name, type);

            if (!overwrite && _parameterResolvers.ContainsKey(matchedParameter.Name))
            {
                throw new InvalidOperationException(
                    $"Parameter '{matchedParameter.Name}' ({matchedParameter.ParameterType}) is already configured"
                );
            }

            _parameterResolvers[matchedParameter.Name] = resolver;
        }
    }
}