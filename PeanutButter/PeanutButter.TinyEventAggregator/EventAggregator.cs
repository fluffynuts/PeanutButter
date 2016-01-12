using System;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.TinyEventAggregator
{
    public class EventAggregator
    {
        private static object _lock = new object();
        private static EventAggregator _instance;
        private List<EventBase> _events;

        public static EventAggregator Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new EventAggregator();
                    return _instance;
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
                if (match == null)
                {
                    match = Activator.CreateInstance<TEvent>();
                    _events.Add(match);
                }
                return match as TEvent;
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
