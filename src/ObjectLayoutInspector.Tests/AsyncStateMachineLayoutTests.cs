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

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
        public async ValueTask<int> WithValueTask()
        {
            await Task.Yield();
            return 42;
        }
#endif
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

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
        [Test]
        public void AsyncValueTaskStateMachineLayout()
        {
            var (_, valueTask) = GetStateMachineTypes();
            TypeLayout.PrintLayout(valueTask);
        }
#endif

        private static (Type taskStateMachine, Type valueTaskStateMachine) GetStateMachineTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                t.FullName!.Contains("AsyncSample") && t.FullName.Contains(">d__")).ToList();

            var taskStateMachine = types.First(t => t.FullName!.Contains(nameof(AsyncSample.WithTask)));

#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            var valueTaskStateMachine = types.First(t => t.FullName!.Contains(nameof(AsyncSample.WithValueTask)));

            return (taskStateMachine, valueTaskStateMachine);
#else
            return (taskStateMachine, null!);
#endif
        }
    }
    }