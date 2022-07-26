using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace EmailSpooler.Win32Service.Tests
{
    public class DbSetSubstituteBuilder<T> where T: class
    {
        private ObservableCollection<T> _collection;
        public static DbSetSubstituteBuilder<T> Create()
        {
            return new DbSetSubstituteBuilder<T>();
        }
        public static IDbSet<T> BuildDefault()
        {
            return Create().Build();
        }
        public DbSetSubstituteBuilder()
        {
            WithDataStore(new ObservableCollection<T>());
        }

        public DbSetSubstituteBuilder<T> WithDataStore(ObservableCollection<T> collection)
        {
            _collection = collection;
            return this;
        }

        private IQueryable _asQueryable;
        private IQueryable AsQueryable
        {
            get
            {
                if (_asQueryable == null)
                {
                    lock (this)
                    {
                        _asQueryable = _collection.AsQueryable();
                    }
                }
                return _asQueryable;
            }
        }

        public IDbSet<T> Build()
        {
            var dbset = Substitute.For<IDbSet<T>>();

            MockAdd(dbset);
            MockRemove(dbset);
            MockAttach(dbset);
            dbset.ElementType.Returns(AsQueryable.ElementType);
            dbset.Expression.Returns(AsQueryable.Expression);
            dbset.Provider.Returns(AsQueryable.Provider);
            dbset.GetEnumerator().Returns(_collection.GetEnumerator());

            return dbset;
        }

        private void MockRemove(IDbSet<T> dbset)
        {
            dbset.Remove(Arg.Any<T>()).ReturnsForAnyArgs(a => RemoveItem(a[0] as T));
        }

        private void MockAttach(IDbSet<T> dbset)
        {
            dbset.Attach(Arg.Any<T>()).ReturnsForAnyArgs(a => AddItem(a[0] as T));
        }

        private void MockAdd(IDbSet<T> dbset)
        {
            dbset.Add(Arg.Any<T>()).ReturnsForAnyArgs(a => AddItem(a[0] as T));
        }

        private T AddItem(T item)
        {
            var existing = _collection.FirstOrDefault(i => i == item);
            if (existing == null)
                _collection.Add(item);
            return item;
        }
        private T RemoveItem(T item)
        {
            _collection.Remove(item);
            return item;
        }
    }
}
