﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GRXoft.Extensions.DependencyInjection
{
    public sealed class ConstructorBuilder<TService> : IConstructorBuilder<TService>
    {
        private static readonly MethodInfo _method = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService));

        private readonly ConstructorInfo _constructor;
        private readonly IDictionary<string, Delegate> _parameterResolvers;
        private readonly IReadOnlyList<ParameterInfo> _parameters;

        public ConstructorBuilder() : this(SelectConstructor())
        {
        }

        public ConstructorBuilder(ConstructorInfo constructor)
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

        /// <inheritdoc/>
        public Func<IServiceProvider, TService> Build()
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
            var lambda = Expression.Lambda<Func<IServiceProvider, TService>>(ctor, serviceProvider);

            return lambda.Compile();
        }

        /// <inheritdoc/>
        public void ResolveParameter<T>(string name, Func<IServiceProvider, T> resolver, bool overwrite)
        {
            if (name is string && string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(); // TODO

            var parameter = MatchParameter(typeof(T), name);

            if (!overwrite && _parameterResolvers.ContainsKey(parameter.Name))
                throw new Exception(); // TODO

            _parameterResolvers[parameter.Name] = resolver;
        }

        private static Expression BuildDefaultParameterExpression(ParameterExpression serviceProvider, ParameterInfo parameter)
        {
            var type = Expression.Constant(parameter.ParameterType);
            var expression = (Expression)Expression.Call(serviceProvider, _method, type);
            if (!parameter.ParameterType.Equals(_method.ReturnType))
                expression = Expression.Convert(expression, parameter.ParameterType);

            return expression;
        }

        private static Expression BuildCustomParameterExpression(Expression serviceProvider, ParameterInfo parameter, Delegate resolver)
        {
            var expression = (Expression)Expression.Invoke(Expression.Constant(resolver), serviceProvider);

            if (!parameter.ParameterType.Equals(resolver.Method.ReturnType))
                expression = Expression.Convert(expression, parameter.ParameterType);

            return expression;
        }

        private static ConstructorInfo SelectConstructor()
        {
            var type = typeof(TService);
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

        private ParameterInfo MatchParameter(Type type, string name)
        {
            var matchingParameters = _parameters.Where(p => p.ParameterType.Equals(type));

            if (name is string)
                matchingParameters = matchingParameters.Where(p => p.Name.Equals(name, StringComparison.Ordinal));

            var enumerator = matchingParameters.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new Exception(); // TODO: No match

            var matchedParameter = enumerator.Current;

            if (enumerator.MoveNext())
                throw new Exception(); // TODO: Ambiguous match

            return matchedParameter;
        }
    }
}