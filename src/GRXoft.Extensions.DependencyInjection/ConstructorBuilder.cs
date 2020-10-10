using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GRXoft.Extensions.DependencyInjection
{
    internal class ConstructorBuilder<TService> : IConstructorBuilder<TService>
    {
        private readonly ConstructorInfo _constructor;
        private readonly IReadOnlyList<ParameterInfo> _parameters;
        private readonly IDictionary<string, ParameterValueProvider> _parameterValues;

        public ConstructorBuilder()
        {
            var type = typeof(TService);
            if (type.IsInterface || type.IsAbstract)
                throw new Exception(); // TODO

            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (ctors.Length == 0)
                throw new Exception(); // TODO

            if (ctors.Length > 1)
                throw new Exception(); // TODO

            _constructor = ctors[0];
            _parameters = _constructor.GetParameters();

            foreach (var parameter in _parameters)
            {
                if (parameter.IsOut || parameter.ParameterType.IsByRef)
                    throw new Exception(); // TODO
            }

            _parameterValues = new Dictionary<string, ParameterValueProvider>();
        }

        public Func<IServiceProvider, TService> Build()
        {
            throw new NotImplementedException();
        }

        private ParameterInfo MatchParameter(string name)
        {
            var matches = _parameters.Where(p => p.Name.Equals(name, StringComparison.Ordinal)).GetEnumerator();

            if (!matches.MoveNext())
                throw new Exception(); // TODO: No match

            var match = matches.Current;

            if (matches.MoveNext())
                throw new Exception(); // TODO: Ambiguous match

            return match;
        }

        private ParameterInfo MatchParameter(Type type)
        {
            var matches = _parameters.Where(p => p.ParameterType.Equals(type)).GetEnumerator();

            if (!matches.MoveNext())
                throw new Exception(); // TODO: No match

            var match = matches.Current;

            if (matches.MoveNext())
                throw new Exception(); // TODO: Ambiguous match

            return match;
        }

        private abstract class ParameterValueProvider
        {
        }
    }
}