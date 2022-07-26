using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DryIoc;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer
{
    public class Bootstrapper
    {
        public IResolvingContainer Bootstrap()
        {
            var container = new Container(
                rules => rules.WithoutThrowOnRegisteringDisposableTransient()
            );
            var allTypes = typeof(Bootstrapper).Assembly.GetTypes()
                .Where(t => !string.IsNullOrWhiteSpace(t.Namespace))
                .Where(t => !t.Namespace.StartsWith("DryIoc"))
                .ToArray();
            var interfaces = allTypes.Where(t => t.IsInterface).ToArray();
            var implementations = allTypes.Where(t => !t.IsAbstract && !t.IsInterface)
                .ToArray();
            foreach (var iface in interfaces)
            {
                var possibleImplementations = new List<Type>();
                foreach (var impl in implementations)
                {
                    if (impl.Implements(iface))
                    {
                        possibleImplementations.Add(impl);
                    }
                }

                if (possibleImplementations.Count == 1)
                {
                    container.Register(
                        iface,
                        possibleImplementations[0],
                        Reuse.Transient);
                }
            }

            return new ResolvingContainer(container);
        }
    }
}