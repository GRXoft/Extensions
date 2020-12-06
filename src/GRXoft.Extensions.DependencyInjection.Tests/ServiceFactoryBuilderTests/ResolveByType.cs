using FluentAssertions;
using NUnit.Framework;
using System;

namespace GRXoft.Extensions.DependencyInjection.ServiceFactoryBuilderTests
{
    [TestFixture]
    internal class ResolveByType
    {
        [Test]
        public void ShouldCorrectlyResolveParameter()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(Tuple<string>));

            // Act
            sut.Resolve(type: typeof(string), resolver: sp => "A test value");

            // Assert
            var tuple = (Tuple<string>)sut.Build().Invoke(null);
            tuple.Item1.Should().Be("A test value");
         }

        [Test]
        public void ShouldCorrectlyResolveReconfiguredParameter()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(Tuple<string>))
                .Resolve(type: typeof(string), resolver: sp => "A test value");

            // Act
            sut.Resolve(type: typeof(string), resolver: sp => "Another test value", overwrite: true);

            // Assert
            var tuple = (Tuple<string>)sut.Build().Invoke(null);
            tuple.Item1.Should().Be("Another test value");
        }

        [Test]
        public void ShouldThrowArgumentExceptionWhenMultipleMatchingParameters()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(Tuple<string, string>));

            // Act
            var act = sut.Invoking(x => x.Resolve(type: typeof(string), resolver: sp => "test"));

            // Assert
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("Multiple parameters match specified criteria");
        }

        [Test]
        public void ShouldThrowArgumentExceptionWhenNoMatchingParameter()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = sut.Invoking(x => x.Resolve(type: typeof(string), resolver: sp => "test"));

            // Assert
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("No parameters match specified criteria");
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionForResolver()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = FluentActions.Invoking(() => sut.Resolve(type: typeof(object), resolver: null));

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("resolver");
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionForType()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(object));

            // Act
            var act = sut.Invoking(x => x.Resolve(type: null, resolver: sp => default));

            // Assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");
        }

        [Test]
        public void ShouldThrowInvalidOperationExceptionWhenReconfiguringWithoutOverwrite()
        {
            // Arrange
            var sut = new ServiceFactoryBuilder(typeof(Tuple<string>))
                .Resolve(type: typeof(string), resolver: sp => "text1");

            // Act
            var act = sut.Invoking(x => x.Resolve(type: typeof(string), resolver: sp => "text2", overwrite: false));

            // Assert
            act.Should().Throw<InvalidOperationException>().And.Message.Should().Contain("already configured");
        }
    }
}