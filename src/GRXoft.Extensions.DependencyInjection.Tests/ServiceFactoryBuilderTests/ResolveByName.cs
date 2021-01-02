using FluentAssertions;
using NUnit.Framework;
using System;

namespace GRXoft.Extensions.DependencyInjection.ServiceFactoryBuilderTests
{
    [TestFixture]
    internal class ResolveByNmae
    {
        [Test]
        public void ShouldCorrectlyResolveParameter()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(Tuple<string>));

            // Act
            sut.Resolve(name: "item1", resolver: sp => "A test value");

            // Assert
            var tuple = (Tuple<string>)sut.Build().Invoke(null);
            tuple.Item1.Should().Be("A test value");
         }

        [Test]
        public void ShouldCorrectlyResolveReconfiguredParameter()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(Tuple<string>))
                .Resolve(name: "item1", resolver: sp => "A test value");

            // Act
            sut.Resolve(name: "item1", resolver: sp => "Another test value", overwrite: true);

            // Assert
            var tuple = (Tuple<string>)sut.Build().Invoke(null);
            tuple.Item1.Should().Be("Another test value");
        }

        [Test]
        public void ShouldThrowArgumentExceptionWhenNoMatchingParameter()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = sut.Invoking(x => x.Resolve(name: "item1", resolver: sp => "test"));

            // Assert
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("No parameters match specified criteria");
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionForResolver()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = FluentActions.Invoking(() => sut.Resolve(name: "item1", resolver: null));

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("resolver");
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionForName()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = sut.Invoking(x => x.Resolve(name: null, resolver: sp => default));

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
        }

        [Test]
        public void ShouldThrowInvalidOperationExceptionWhenReconfiguringWithoutOverwrite()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(Tuple<string>))
                .Resolve(name: "item1", resolver: sp => "text1");

            // Act
            var act = sut.Invoking(x => x.Resolve(name: "item1", resolver: sp => "text2", overwrite: false));

            // Assert
            act.Should().Throw<InvalidOperationException>().And.Message.Should().Contain("already configured");
        }
    }
}