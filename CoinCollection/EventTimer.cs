using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CoinCollection
{
    public enum EventTimerType
    {
        milliseconds,
        seconds,
        minutes,
        hours
    }

    public abstract class EventTimerActionBase
    {
        public abstract void Invoke();
    }

    public class EventTimerAction(Action action) : EventTimerActionBase
    {
        private readonly Action _action = action;

        public override void Invoke()
        {
            _action();
        }
    }

    public class EventTimerActionGeneric<T>(T? key, Action<T?> action) where T : notnull
    {
        private readonly T? _key = key;

        private readonly Action<T?> _action = action;

        public EventTimerActionGeneric(Action<T?> action) : this(default, action) { }

        public void Invoke()
        {
            _action(_key);
        }
    }


    internal class EventTimer : IDisposable
    {
        private readonly System.Timers.Timer _timer;

        private readonly EventTimerActionBase[] _actions;

        private readonly double _amount;

        public EventTimer(double amount = 1000, EventTimerType eventTimerType = EventTimerType.milliseconds, bool timeLineup = true, params EventTimerActionBase[] actions)
        {
            _timer = new()
            {
                AutoReset = false
            };

            if (eventTimerType == EventTimerType.milliseconds && amount < 1000)
            {
                throw new ArgumentException("Millisecond amount can not be any lower than a second");
            }

            switch(eventTimerType)
            {
                case EventTimerType.milliseconds:
                    _amount = amount;
                    break;
                case EventTimerType.seconds:
                    _amount = TimeSpan.FromSeconds(amount).TotalMilliseconds;
                    break;
                case EventTimerType.minutes:
                    _amount = TimeSpan.FromMinutes(amount).TotalMilliseconds;
                    break;
                case EventTimerType.hours:
                    _amount = TimeSpan.FromHours(amount).TotalMilliseconds;
                    break;
            }

            double adjustedTime = _amount;

            if(timeLineup)
            {
                //TODO: Do some more tests
                if(eventTimerType == EventTimerType.minutes || eventTimerType == EventTimerType.hours)
                {
                    DateTime now = DateTime.Now;
                    int alignment = (int)amount;

                    DateTime nextAlignment;

                    if (eventTimerType == EventTimerType.minutes)
                    {
                        int alignmentMin = ((now.Minute / alignment) + 1) * alignment;

                        nextAlignment = now.Date.AddHours(now.Hour).AddMinutes(alignmentMin % 60);

                        if (alignmentMin >= 60)
                        {
                            nextAlignment = nextAlignment.AddHours(1);
                        }
                    }
                    else
                    {
                        int alignmentHour = ((now.Hour / alignment) + 1) * alignment;

                        nextAlignment = now.Date.AddHours(alignmentHour % 24);

                        if (alignmentHour >= 24)
                        {
                            nextAlignment = nextAlignment.AddDays(1);
                        }
                    }

                    TimeSpan remainingTime = nextAlignment - now;
                    adjustedTime = remainingTime.TotalMilliseconds;
                }
            }

            _actions = actions;

            _timer.Interval = adjustedTime;
            _timer.Elapsed += EventTigger;
            _timer.Enabled = true;
        }

        private void EventTigger(object? sender, ElapsedEventArgs e)
        {
            if(!_timer.AutoReset)
            {
                _timer.Stop();
                _timer.Interval = _amount;
                _timer.AutoReset = true;
                _timer.Start();
            }

            foreach (var action in _actions)
            {
                action.Invoke();
            }
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
