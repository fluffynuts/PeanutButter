using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TinyEventAggregator
{
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

    public class EventAggregator
        : IEventAggregator
    {
        private static readonly object Lock = new object();
        private static EventAggregator _instance;
        private readonly List<EventBase> _events;

        public static EventAggregator Instance
        {
            get
            {
                lock (Lock)
                {
                    return _instance ?? (_instance = new EventAggregator());
                }
            }
        }

        public EventAggregator()
        {
            _events = new List<EventBase>();
        }

        public TEvent GetEvent<TEvent>() where TEvent: EventBase, new()
        {
            lock (this)
            {
                var match = _events.FirstOrDefault(ev => (ev as TEvent) != null);
                if (match != null)
                    return match as TEvent;
                match = Activator.CreateInstance<TEvent>();
                _events.Add(match);
                return (TEvent)match;
            }
        }

        public void Unsuspend()
        {
            foreach (var ev in _events)
                ev.Suspend();
        }

        public void Suspend()
        {
            foreach (var ev in _events)
                ev.Unsuspend();
        }
    }
}
