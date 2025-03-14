// <copyright file="EventTimer.cs" company="Karatekid2501">
// Copyright (c) Karatekid2501. All rights reserved.
// </copyright>

using System.Timers;

namespace CoinCollection
{
    /// <summary>
    /// Type of event timer.
    /// </summary>
    public enum EventTimerType
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
#pragma warning restore SA1602 // Enumeration items should be documented
    }

    /// <summary>
    /// Interface for the EventTimerAction classes.
    /// </summary>
    public interface IEventTimerActionBase
    {
        /// <summary>
        /// Invoke the action.
        /// </summary>
        public void Invoke();
    }

    /// <summary>
    /// Sets up an empty action to use once the timer is up.
    /// </summary>
    /// <param name="action">Action to trigger once the timer is up.</param>
    public class EventTimerAction(Action action) : IEventTimerActionBase
    {
        private readonly Action _action = action;

        /// <inheritdoc/>
        public void Invoke()
        {
            _action();
        }
    }

    /// <summary>
    /// Sets up an action that uses a generic to use once the timer is up.
    /// </summary>
    /// <typeparam name="T">Generic type to use for key and action.</typeparam>
    /// <param name="key">Key to use in the action.</param>
    /// <param name="action">Action to trigger once the timer is up.</param>
    public class EventTimerActionGeneric<T>(T? key, Action<T?> action) : IEventTimerActionBase
        where T : notnull
    {
        private readonly T? _key = key;

        private readonly Action<T?> _action = action;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTimerActionGeneric{T}"/> class.
        /// </summary>
        /// <param name="action">Action to trigger once the timer is up.</param>
        public EventTimerActionGeneric(Action<T?> action)
            : this(default, action)
        {
        }

        /// <inheritdoc/>
        public void Invoke()
        {
            _action(_key);
        }
    }

    /// <summary>
    /// Timer to trigger event once the timer is up.
    /// </summary>
    internal class EventTimer : IDisposable
    {
        private readonly System.Timers.Timer _timer;

        private readonly IEventTimerActionBase[] _actions;

        private readonly double _amount;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTimer"/> class.
        /// </summary>
        /// <param name="amount">How much time the timer will count down from.</param>
        /// <param name="eventTimerType">Type of timer to use.</param>
        /// <param name="timeLineup">Should the timer line up to exact or start immediately.</param>
        /// <param name="actions">All actions that will take place when the timer is up.</param>
        public EventTimer(double amount = 1000, EventTimerType eventTimerType = EventTimerType.Milliseconds, bool timeLineup = true, params IEventTimerActionBase[] actions)
        {
            _timer = new()
            {
                AutoReset = false,
            };

            if (eventTimerType == EventTimerType.Milliseconds && amount < 1000)
            {
                throw new ArgumentException("Millisecond amount can not be any lower than a second");
            }

            switch (eventTimerType)
            {
                case EventTimerType.Milliseconds:
                    _amount = amount;
                    break;
                case EventTimerType.Seconds:
                    _amount = TimeSpan.FromSeconds(amount).TotalMilliseconds;
                    break;
                case EventTimerType.Minutes:
                    _amount = TimeSpan.FromMinutes(amount).TotalMilliseconds;
                    break;
                case EventTimerType.Hours:
                    _amount = TimeSpan.FromHours(amount).TotalMilliseconds;
                    break;
            }

            double adjustedTime = _amount;

            if (timeLineup)
            {
                // TODO: Do some more tests
                if (eventTimerType == EventTimerType.Minutes || eventTimerType == EventTimerType.Hours)
                {
                    DateTime now = DateTime.Now;
                    int alignment = (int)amount;

                    DateTime nextAlignment;

                    if (eventTimerType == EventTimerType.Minutes)
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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of variables when class is finished with.
        /// </summary>
        /// <param name="disposing">Should the class dispose when this method is called.</param>
        protected virtual void Dispose(bool disposing)
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void EventTigger(object? sender, ElapsedEventArgs e)
        {
            if (!_timer.AutoReset)
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
    }
}
