using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectLayoutInspector.Tests
{
    public class AsyncSample
    {
        public async Task<int> WithTask()
        {
            await Task.Yield();
            return 42;
        }

        public async ValueTask<int> WithValueTask()
        {
            await Task.Yield();
            return 42;
        }
    }

    [TestFixture]
    public class AsyncStateMachineLayoutTests
    {
        [Test]
        public void AsyncTaskStateMachineLayout()
        {
            var (taskStateMachine, _) = GetStateMachineTypes();
            TypeLayout.PrintLayout(taskStateMachine);
        }

        [Test]
        public void AsyncValueTaskStateMachineLayout()
        {
            var (_, valueTask) = GetStateMachineTypes();
            TypeLayout.PrintLayout(valueTask);
        }

        private static (Type taskStateMachine, Type valueTaskStateMachine) GetStateMachineTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                t.FullName.Contains("AsyncSample") && t.FullName.Contains(">d__")).ToList();

            var taskStateMachine = types.First(t => t.FullName.Contains(nameof(AsyncSample.WithTask)));
            var valueTaskStateMachine = types.First(t => t.FullName.Contains(nameof(AsyncSample.WithValueTask)));
            return (taskStateMachine, valueTaskStateMachine);
        }
    }
}