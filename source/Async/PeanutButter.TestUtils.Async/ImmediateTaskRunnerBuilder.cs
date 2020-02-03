using System;
using System.Threading.Tasks;
using NSubstitute;
using PeanutButter.Async.Interfaces;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.Async
{
    public class ImmediateTaskRunnerBuilder :
        GenericBuilder<ImmediateTaskRunnerBuilder, ITaskRunner>
    {
        private ITaskRunner _taskRunner;

        public ImmediateTaskRunnerBuilder()
        {
            WithTaskRunner(Substitute.For<ITaskRunner>())
                .WithDefaultHandlers();
        }

        public ImmediateTaskRunnerBuilder WithDefaultHandlers()
        {
            _taskRunner.Run(Arg.Any<Action>())
                .ReturnsForAnyArgs(ci =>
                {
                    var task = Task.Run(ci.Arg<Action>());
                    task.Wait();
                    return task;
                });
            _taskRunner.CreateNotStartedFor(Arg.Any<Action>())
                .ReturnsForAnyArgs(ci => new Task(ci.Arg<Action>()));
            _taskRunner.Continue(Arg.Any<Task>())
                .ReturnsForAnyArgs(ci => CreateImmediateContinuationFor(ci.Arg<Task>()));
            return WithSupportForTaskOfType<bool>();
        }

        private IContinuation CreateImmediateContinuationFor(Task initialTask)
        {
            var continuation = Substitute.For<IContinuation>();
            continuation.With(Arg.Any<Action<Task>>())
                .ReturnsForAnyArgs(ci =>
                {
                    var action = ci.Arg<Action<Task>>();
                    var nextTask = Task.Run(() => action(initialTask));
                    nextTask.Wait();
                    return nextTask;
                });
            return continuation;
        }

        private IContinuation<T> CreateImmediateContinuationFor<T, TNext>(Task<T> initialTask)
        {
            var continuation = Substitute.For<IContinuation<T>>();
            continuation.With(Arg.Any<Func<Task<T>, TNext>>())
                .ReturnsForAnyArgs(ci =>
                {
                    initialTask.Wait();
                    var action = ci.Arg<Func<Task<T>, TNext>>();
                    var nextTask = Task.Run(() => action(initialTask));
                    nextTask.Wait();
                    return nextTask;
                });
            return continuation;
        }

        public ImmediateTaskRunnerBuilder WithNotStartedHandlerFor<T>()
        {
            _taskRunner.CreateNotStartedFor(Arg.Any<Func<T>>())
                .ReturnsForAnyArgs(ci => new Task<T>(ci.Arg<Func<T>>()));
            return this;
        }

        public ImmediateTaskRunnerBuilder WithTaskRunner(ITaskRunner taskRunner)
        {
            _taskRunner = taskRunner;
            return this;
        }

        public ImmediateTaskRunnerBuilder WithSupportForTaskOfType<T>()
        {
            _taskRunner.Run(Arg.Any<Func<T>>())
                .ReturnsForAnyArgs(ci =>
                {
                    var task = Task.Run(() => ci.Arg<Func<T>>()());
                    task.Wait();
                    return task;
                });
            _taskRunner.CreateNotStartedFor(Arg.Any<Func<T>>())
                .ReturnsForAnyArgs(ci => new Task<T>(ci.Arg<Func<T>>()));
            return this;
        }

        public ImmediateTaskRunnerBuilder WithSupportForContinuationOfType<T, TNext>()
        {
            _taskRunner.Continue(Arg.Any<Task<T>>())
                .ReturnsForAnyArgs(ci => CreateImmediateContinuationFor<T, TNext>(ci.Arg<Task<T>>()));
            return this;
        }

        public override ITaskRunner Build()
        {
            return _taskRunner;
        }
    }
}
