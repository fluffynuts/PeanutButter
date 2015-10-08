using System;
using System.Collections.Generic;

namespace PeanutButter.Reflection
{
    public interface ITypeEnumeratorFilter : IEnumerable<Type>
    {
        ITypeEnumeratorFilterCriteria AndAre { get; }
        ITypeEnumeratorFilterCriteria AndNot { get; }
    }
}
