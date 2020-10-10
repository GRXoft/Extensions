using System;

namespace GRXoft.Extensions.DependencyInjection
{
    public interface IConstructorBuilder<TService>
    {
        Func<IServiceProvider, TService> Build();

        IConstructorBuilder<TService> Parameter<T>(Func<IServiceProvider, T> valueProvider);

        IConstructorBuilder<TService> Parameter<T>(string name, Func<IServiceProvider, T> valueProvider);
    }
}