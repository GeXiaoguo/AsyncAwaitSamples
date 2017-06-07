using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Determining_RunningThread_Tests
    {
        /// <summary>
        /// TPL will flow thread ambient information when starting a new tasks
        /// Ambient information include: ExecutionContext, CurrentCuture, ...
        /// CurrentCuture is not flown to the new task in .NET 4.5
        /// It is flown to the new task in .NET 4.6
        /// https://blogs.msdn.microsoft.com/pfxteam/2012/06/15/executioncontext-vs-synchronizationcontext/
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CurrentCulture_IsNot_Flowed_To_NewThread()
        {
            CultureInfo ci = new CultureInfo("fr-FR");
            Thread.CurrentThread.CurrentCulture = ci;

            PrintThreadAmbientInfo($"state0");

            await DoWorkAsyncFor100ms();

            PrintThreadAmbientInfo($"state1");
        }


        /// <summary>
        /// Console program starts with a MainThread
        /// Tasks started in MainThread captures the SynchronizationContext of MainThread which is null
        /// Continuation continues on captured SynchronizationContext which is null
        /// Continueation starts on thread pool thread.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsyncAwait_ContinueOn_ThreadPoolThread()
        {
            PrintThreadAmbientInfo("state0");

            var task = DoWorkAsyncFor100ms();

            await task;

            PrintThreadAmbientInfo("state1");

            task = DoWorkAsyncFor100ms();

            await task;

            PrintThreadAmbientInfo("state2");
        }


        /// <summary>
        /// callback resumes on the calling thread when the task has already completed at the time of await
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsyncAwait_ContinueOn_CallerThread()
        {
            PrintThreadAmbientInfo("state0");

            var task = DoWorkAsyncFor100ms();

            Thread.Sleep(200);
            await task;

            PrintThreadAmbientInfo("state1");

            task = DoWorkAsyncFor100ms();

            Thread.Sleep(200);
            await task;

            PrintThreadAmbientInfo("state2");
        }

        private static Task<int> DoWorkAsyncFor100ms()
        {
            var task = Task.Run(() =>
            {
                PrintThreadAmbientInfo("Task Running in:");
                Thread.Sleep(100);
                return 1;
            });
            return task;
        }

        private static void PrintThreadAmbientInfo(string name)
        {
            Console.WriteLine($"{name} -------------------------------------");
            Console.WriteLine(value: $"{name} CurrentSynchronizationContext:{SynchronizationContext.Current?.GetType()?.ToString() ?? "null"}");
            Console.WriteLine($"{name} CurrentThread:{Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"{name} CurrentCulture:{CultureInfo.CurrentCulture}");
        }
    }
}
