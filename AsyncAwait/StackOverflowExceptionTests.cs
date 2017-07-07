using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncAwait
{
    [TestFixture]
    public class StackOverflowExceptionTests
    {

        [Test]
        public void StackOverflowException_WithoutException_Test()
        {
            RecursiveFunctionWithoutExceptino(0, 5000).GetAwaiter().GetResult();
        }

        [Test]
        public void StackOverflowException_WithException_Test()
        {
            RecursiveFunctionWithException(0, 1000).GetAwaiter().GetResult();
        }

        static async Task<string> RecursiveFunctionWithException(int index, int max)
        {
            await Task.Yield();

            if (index < max)
            {
                index++;
                try
                {
                    Console.WriteLine($"b {index} of {max} (on threadId: {Thread.CurrentThread.ManagedThreadId})");
                    return await RecursiveFunctionWithException(index, max).ConfigureAwait(false);
                }
                finally
                {
                    Console.WriteLine($"e {index} of {max} (on threadId: {Thread.CurrentThread.ManagedThreadId})");
                }
            }
            throw new Exception("");
        }

        static async Task<string> RecursiveFunctionWithoutExceptino(int index, int max)
        {
            await Task.Yield();

            await Task.Run(() => Thread.Sleep(100));

            if (index < max)
            {
                index++;
                Console.WriteLine($"b {index} of {max} (on threadId: {Thread.CurrentThread.ManagedThreadId})");
                return await RecursiveFunctionWithoutExceptino(index, max).ConfigureAwait(false);
            }

            return "hello";
        }
    }
}
