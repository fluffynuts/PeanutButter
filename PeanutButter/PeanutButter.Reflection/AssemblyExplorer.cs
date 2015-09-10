using System;
using System.Collections.Generic;
using System.Reflection;

namespace PeanutButter.Reflection
{
    public class AssemblyExplorer : ICloneable
    {
        private bool _reflectionOnly;
        private HashSet<Assembly> _addedAssemblies;
        private IReadOnlyList<Type> _readOnlyTypes;
        private Type[] _types;

        public IReadOnlyList<Type> Types { get { return _readOnlyTypes; } }

        private AssemblyExplorer(AssemblyExplorer original)
        {
            _addedAssemblies = new HashSet<Assembly>(original._addedAssemblies);
            _types = original._types;
            _readOnlyTypes = new ArraySegment<Type>(_types);
            _reflectionOnly = original._reflectionOnly;
        }

        public AssemblyExplorer()
        {
            _types = new Type[0];
            _readOnlyTypes = new ArraySegment<Type>(_types);
            _addedAssemblies = new HashSet<Assembly>();
        }

        public void AddAssembly<TTypeContainedInAssembly>()
        {
            AddAssembly(typeof(TTypeContainedInAssembly).Assembly);
        }

        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            if (_addedAssemblies.Add(assembly))
            {
                Type[] types = assembly.ReflectionOnly ? assembly.GetExportedTypes() : assembly.GetTypes();
                Type[] nTypes = new Type[types.Length + _types.Length];

                Array.Copy(_types, nTypes, _types.Length);
                Array.Copy(types, 0, nTypes, _types.Length, types.Length);

                _types = nTypes;
                _readOnlyTypes = new ArraySegment<Type>(_types);

                if (assembly.ReflectionOnly)
                    _reflectionOnly = true;
            }
        }

        public ITypeEnumeratorFilterCriteria FilterTypes()
        {
            return new TypeEnumeratorFilter(_types, _reflectionOnly);
        }

        public AssemblyExplorer Clone()
        {
            return new AssemblyExplorer(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
