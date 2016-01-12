using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NSubstitute;

namespace EACH.Tests.Builders
{
    public class IDbSetSubstituteBuilder<T> where T: class
    {
        private ObservableCollection<T> _collection;
        public static IDbSetSubstituteBuilder<T> Create()
        {
            return new IDbSetSubstituteBuilder<T>();
        }
        public static IDbSet<T> BuildDefault()
        {
            return Create().Build();
        }
        public IDbSetSubstituteBuilder()
        {
            WithDataStore(new ObservableCollection<T>());
        }

        public IDbSetSubstituteBuilder<T> WithDataStore(ObservableCollection<T> collection)
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
        private IEnumerator<T> _enumerator;
        private IEnumerator<T> Enumerator
        {
            get
            {
                if (_enumerator == null)
                {
                    lock (this)
                    {
                        _enumerator = _collection.GetEnumerator();
                    }
                }
                return _enumerator;
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
