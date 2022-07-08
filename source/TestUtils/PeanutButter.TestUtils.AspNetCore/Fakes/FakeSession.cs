using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public Task LoadAsync(
            CancellationToken cancellationToken = new()
        )
        {
            return Task.CompletedTask;
        }

        public Task CommitAsync(
            CancellationToken cancellationToken = new()
        )
        {
            return Task.CompletedTask;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            return _store.TryGetValue(key, out value);
        }

        public void Set(string key, byte[] value)
        {
            _store[key] = value;
        }

        public void Remove(string key)
        {
            _store.Remove(key);
        }

        public void Clear()
        {
            _store.Clear();
        }

        public bool IsAvailable { get; } = true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;
    }
}