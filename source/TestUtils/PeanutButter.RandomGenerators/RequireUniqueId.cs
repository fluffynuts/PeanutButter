using System;
using System.Linq;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Require a unique Id on the generated entity
    /// </summary>
    public class RequireUniqueId : RequireUnique
    {
        /// <inheritdoc />
        public RequireUniqueId(): base("Id")
        {
        }

        /// <inheritdoc />
        public override void Init(Type entityType)
        {
            var propName = PropertyNames.Single();
            PropertyType = entityType.GetProperty(propName)
                ?.PropertyType ?? throw new InvalidOperationException(
                               $"Unable to find property {propName} on {entityType}"
                );
            base.Init(entityType);
        }
    }
}