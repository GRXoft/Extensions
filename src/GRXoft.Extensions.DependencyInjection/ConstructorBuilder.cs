﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GRXoft.Extensions.DependencyInjection
{
    public sealed class ConstructorBuilder
    {
        private static readonly MethodInfo _method = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService));

        private readonly ConstructorInfo _constructor;
        private readonly IDictionary<string, Delegate> _parameterResolvers;
        private readonly IReadOnlyList<ParameterInfo> _parameters;

        public ConstructorBuilder(Type type) : this(SelectConstructor(type))
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

        public ConstructorBuilder Resolve(string name, Func<IServiceProvider, object> resolver, bool overwrite)
        {
            if (name is string && string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(); // TODO

            var parameter = MatchParameter(name, null);

            if (!overwrite && _parameterResolvers.ContainsKey(parameter.Name))
                throw new Exception(); // TODO

            _parameterResolvers[parameter.Name] = resolver;

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
                throw new Exception(); // TODO: No match

            var matchedParameter = enumerator.Current;

            if (enumerator.MoveNext())
                throw new Exception(); // TODO: Ambiguous match

            return matchedParameter;
        }
    }
}