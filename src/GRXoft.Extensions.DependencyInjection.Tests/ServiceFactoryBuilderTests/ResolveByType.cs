using FluentAssertions;
using NUnit.Framework;
using System;

namespace GRXoft.Extensions.DependencyInjection.ServiceFactoryBuilderTests
{
    [TestFixture]
    internal class ResolveByType
    {
        [Test]
        public void ThrowArgumentNullExceptionForResolver()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = FluentActions.Invoking(() => sut.Resolve(type: typeof(object), resolver: null));

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("resolver");
        }

        [Test]
        public void ThrowArgumentNullExceptionForType()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = FluentActions.Invoking(() => sut.Resolve(type: null, resolver: sp => default));

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");
        }
    }
}