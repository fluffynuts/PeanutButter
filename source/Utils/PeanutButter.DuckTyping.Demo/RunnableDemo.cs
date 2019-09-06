using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Demo
{
    public class RunnableDemo
    {
        public Type Type { get; }
        public string Name { get; }
        public string Description { get; }
        public int Order { get; }

        public RunnableDemo(Type demoType)
        {
            Type = demoType;
            Order = demoType.GetCustomAttributes()
                .OfType<OrderAttribute>()
                .FirstOrDefault()
                ?.Order ?? 0;
            Name = demoType.Name.ToWords();
            Description = demoType.GetCustomAttributes()
                .OfType<DescriptionAttribute>()
                .FirstOrDefault()
                ?.Description?.Trim() ?? "";
        }
    }
}