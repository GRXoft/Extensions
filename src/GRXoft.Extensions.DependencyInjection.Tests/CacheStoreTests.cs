using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GRXoft.Extensions.DependencyInjection
{
    [TestFixture]
    internal class CacheStoreTests
    {
        private CacheStore<string> _sut = default!;

        [SetUp]
        public void @SetUp()
        {
            _sut = new CacheStore<string>();
        }

        [Test]
        public async Task Read_ShouldReturnCurrentValue()
        {
            // Arrange
            var currentValueTask = Task.FromResult("Ready value");
            await _sut.Update(currentValueTask, default);

            // Act
            var result = await _sut.Read(default);

            // Assert
            result.Should().Be("Ready value");
        }

        [Test]
        public async Task Read_ShouldReturnCurrentValue_WhenUpdateFailed()
        {
            // Arrange
            var currentValueTask = Task.FromResult("Current value");
            _ = _sut.Update(currentValueTask, default);
            var nextValueTask = Task.FromException<string>(new Exception("Some exception"));
            _ = _sut.Update(nextValueTask, default);

            // Act
            var result = await _sut.Read(default);

            // Assert
            result.Should().Be("Current value");
        }

        [Test]
        public async Task Read_ShouldReturnCurrentValue_WhenUpdateIsInProgress()
        {
            // Arrange
            var currentValueTask = Task.FromResult("Current value");
            await _sut.Update(currentValueTask, default);
            var nextValueTaskSource = new TaskCompletionSource<string>();
            _ = _sut.Update(nextValueTaskSource.Task, default);

            // Act
            var result = await _sut.Read(default);

            // Assert
            result.Should().Be("Current value");
        }

        [Test]
        public async Task Read_ShouldReturnUpdatedValue_WhenCurrentValueIsNotAvailable_AndUpateIsInProgress()
        {
            // Arrange
            var nextValueTaskSource = new TaskCompletionSource<string>();
            _ = _sut.Update(nextValueTaskSource.Task, default);
            var readTask = _sut.Read(default);

            // Sanity check
            readTask.IsCompleted.Should().BeFalse();

            // Act
            nextValueTaskSource.SetResult("Updated value");
            var result = await readTask;

            // Assert
            result.Should().Be("Updated value");
        }

        [Test]
        public async Task Read_ShouldThrow_WhenCurrentValueIsNotAvailable_AndUpateIsInProgress_ButCancellationIsRequested()
        {
            // Arrange
            var nextValueTaskSource = new TaskCompletionSource<string>();
            _ = _sut.Update(nextValueTaskSource.Task, default);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var act = _sut.Awaiting(x => x.Read(cancellationTokenSource.Token));
            cancellationTokenSource.Cancel();

            // Assert
            (await act.Should().ThrowAsync<OperationCanceledException>())
                .Which.CancellationToken.Should().Be(cancellationTokenSource.Token);
        }

        [Test]
        public async Task Read_ShouldThrow_WhenCurrentValueIsNotAvailable_AndUpdateFailed()
        {
            // Arrange
            var nextValueTask = Task.FromException<string>(new Exception("Some exception"));
            _ = _sut.Update(nextValueTask, default);

            // Act
            var act = _sut.Awaiting(x => x.Read(default));

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task Read_ShouldThrow_WhenNoValueIsAvailable()
        {
            // Act
            var act = _sut.Awaiting(x => x.Read(default));

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task Update_ShouldDiscardPendingNextValue_WhenSubsequentUpdateWasIssued()
        {
            // Arrange
            var currentValueTask = Task.FromResult("Current value");
            await _sut.Update(currentValueTask, default);
            var nextValueTaskSource = new TaskCompletionSource<string>();
            _ = _sut.Update(nextValueTaskSource.Task, default);
            var subsequentValueTask = Task.FromResult("Subsequent value");

            // Act
            await _sut.Update(subsequentValueTask, default);

            // Assert
            nextValueTaskSource.SetResult("Next value finally finished, but should be ignored in favor of subsequent update");
            (await _sut.Read(default)).Should().Be("Subsequent value");
        }

        [Test]
        public async Task Update_ShouldReplaceCurrentValue()
        {
            // Arrange
            var currentValueTask = Task.FromResult("Current value");
            await _sut.Update(currentValueTask, default);
            var nextValueTask = Task.FromResult("Next value");

            // Sanity check
            (await _sut.Read(default)).Should().Be("Current value");

            // Act
            await _sut.Update(nextValueTask, default);

            // Assert
            (await _sut.Read(default)).Should().Be("Next value");
        }
    }
}
