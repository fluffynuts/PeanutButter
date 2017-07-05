using GenericBuilderTestLoadLoadedAssemblyObject;
using PeanutButter.RandomGenerators;

namespace GenericBuilderTestNotLoadedAssembly
{
    public class NotLoadedBuilder: GenericBuilder<NotLoadedBuilder, ObjectToBuildWithNotLoadedBuilder>
    {
        public override NotLoadedBuilder WithRandomProps()
        {
            return WithProp(o => o.Id = 42);
        }
    }
}
