using System;
using System.ComponentModel;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.Strict
{
    public interface IReadOnlyEntity
    {
        int Id { get; }
        string Name { get; }
    }

    public class LooksLikeAnEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Order(1)]
    [Description("We should be able to read from unrelated types")]
    public class ReadingFromUnrelatedTypes : Demo
    {
        public override void Run()
        {
            var unrelatedObject = new LooksLikeAnEntity()
            {
                Id = 42,
                Name = "Douglas Adams"
            };

            var ducked = unrelatedObject.DuckAs<IReadOnlyEntity>();
            Log("Original:\n", unrelatedObject, "\nDucked:\n", ducked);
        }
    }
}