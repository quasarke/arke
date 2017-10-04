using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Arke.SipEngine.LanguageExtensions
{
    public static class TaskExtensions
    {
        public static void Repeat(this Task taskToRepeat, CancellationToken cancellationToken, TimeSpan intervalTimeSpan)
        {
            var action = taskToRepeat
                .GetType()
                .GetField("m_action", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(taskToRepeat) as Action;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (cancellationToken.WaitHandle.WaitOne(intervalTimeSpan))
                        break;
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    if (action == null)
                        break;
                    Task.Factory.StartNew(action, cancellationToken);
                }
            }, cancellationToken);
        }
    }
}
