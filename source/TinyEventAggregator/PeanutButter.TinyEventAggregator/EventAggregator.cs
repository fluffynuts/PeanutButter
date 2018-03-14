using System;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.TinyEventAggregator
{
    public interface IEventAggregator
    {
        TEvent GetEvent<TEvent>() where TEvent: EventBase, new();
        void Unsuspend();
        void Suspend();
    }

    public class EventAggregator
        : IEventAggregator
    {
        private static readonly object _lock = new object();
        private static EventAggregator _instance;
        private readonly List<EventBase> _events;

        public static EventAggregator Instance
        {
            get
            {
                lock (_lock)
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
