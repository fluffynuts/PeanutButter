using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TinyEventAggregator
{
    /// <summary>
    /// Provides a simple event aggregator
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Retrieves the event so you can subscribe or publish to it.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        TEvent GetEvent<TEvent>() where TEvent: EventBase, new();
        /// <summary>
        /// Suspends event processing. Events are ignored whilst processing is suspended.
        /// </summary>
        void Suspend();
        /// <summary>
        /// Unsuspends event processing.
        /// </summary>
        void Unsuspend();
    }

    /// <inheritdoc />
    public class EventAggregator
        : IEventAggregator
    {
        private static readonly object InstanceLock = new();
        private static EventAggregator _instance;
        private readonly List<EventBase> _events;

        /// <summary>
        /// The singleton instance
        /// </summary>
        public static EventAggregator Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return _instance ??= new EventAggregator();
                }
            }
        }

        /// <summary>
        /// Construct a new Event aggregator
        /// </summary>
        public EventAggregator()
        {
            _events = new List<EventBase>();
        }

        /// <inheritdoc />
        public TEvent GetEvent<TEvent>() where TEvent: EventBase, new()
        {
            lock (_lock)
            {
                var match = _events.FirstOrDefault(ev => (ev as TEvent) != null);
                if (match != null)
                {
                    return match as TEvent;
                }

                match = Activator.CreateInstance<TEvent>();
                _events.Add(match);
                return (TEvent)match;
            }
        }
        
        private readonly object _lock = new();

        /// <inheritdoc />
        public void Unsuspend()
        {
            foreach (var ev in _events)
            {
                ev.Unsuspend();
            }
        }

        /// <inheritdoc />
        public void Suspend()
        {
            foreach (var ev in _events)
            {
                ev.Suspend();
            }
        }
    }
}
