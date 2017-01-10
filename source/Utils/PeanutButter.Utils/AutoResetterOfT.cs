﻿using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a mechanism to run code on construction and disposal, 
    /// irrespective of exception handling
    /// Use this, for example, to set up and tear down state required for
    /// a test -- your constructionAction is called immediately upon construction
    /// and the using() pattern guarantees that your disposalAction is called at
    /// disposal, even if your test fails. 
    /// This is the variant of AutoResetter where:
    /// 1. The start Func returns a value 
    /// 2. Upon disposal, the end action is called with the value from (1)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoResetter<T> : IDisposable
    {
        private readonly T _initialValue;
        private readonly object _lock = new object();
        private Action<T> _disposalAction;

        /// <summary>
        /// Construcst a new AutoResetter, runs the start Func and stores the result
        /// </summary>
        /// <param name="start">Code to run at construction</param>
        /// <param name="end">Code to run at disposal; will receive the result provided from the start Func</param>
        public AutoResetter(Func<T> start, Action<T> end)
        {
            _initialValue = start();
            _disposalAction = end;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposalAction == null)
                    return;
                try
                {
                    _disposalAction(_initialValue);
                }
                finally
                {
                    _disposalAction = null;
                }
            }
        }
    }
}
