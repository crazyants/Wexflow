using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Wexflow.Core
{
    public class WexflowTimer
    {
        public TimerCallback TimerCallback { get; private set; }
        public object State { get; private set; }
        public TimeSpan Period { get; private set; }

        public WexflowTimer(TimerCallback timerCallback, object state, TimeSpan period)
        {
            this.TimerCallback = timerCallback;
            this.State = state;
            this.Period = period;
        }

        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                for(;;)
                {
                    if (stopwatch.ElapsedMilliseconds >= this.Period.TotalMilliseconds)
                    {
                        stopwatch.Reset();
                        stopwatch.Start();
                        this.TimerCallback.Invoke(this.State);
                    }
                    Thread.Sleep(100);
                }
            }));

            thread.Start();
        }
    }
}
