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
            this.WithDataStore(new ObservableCollection<T>());
        }

        public IDbSetSubstituteBuilder<T> WithDataStore(ObservableCollection<T> collection)
        {
            this._collection = collection;
            return this;
        }

        private IQueryable _asQueryable;
        private IQueryable AsQueryable
        {
            get
            {
                if (this._asQueryable == null)
                {
                    lock (this)
                    {
                        this._asQueryable = this._collection.AsQueryable();
                    }
                }
                return this._asQueryable;
            }
        }
        private IEnumerator<T> _enumerator;
        private IEnumerator<T> Enumerator
        {
            get
            {
                if (this._enumerator == null)
                {
                    lock (this)
                    {
                        this._enumerator = this._collection.GetEnumerator();
                    }
                }
                return this._enumerator;
            }
        }

        public IDbSet<T> Build()
        {
            var dbset = Substitute.For<IDbSet<T>>();

            MockAdd(dbset);
            MockRemove(dbset);
            MockAttach(dbset);
            dbset.ElementType.Returns(this.AsQueryable.ElementType);
            dbset.Expression.Returns(this.AsQueryable.Expression);
            dbset.Provider.Returns(this.AsQueryable.Provider);
            dbset.GetEnumerator().Returns(this._collection.GetEnumerator());

            return dbset;
        }

        private void MockRemove(IDbSet<T> dbset)
        {
            dbset.Remove(Arg.Any<T>()).ReturnsForAnyArgs(a => this.RemoveItem(a[0] as T));
        }

        private void MockAttach(IDbSet<T> dbset)
        {
            dbset.Attach(Arg.Any<T>()).ReturnsForAnyArgs(a => this.AddItem(a[0] as T));
        }

        private void MockAdd(IDbSet<T> dbset)
        {
            dbset.Add(Arg.Any<T>()).ReturnsForAnyArgs(a => this.AddItem(a[0] as T));
        }

        private T AddItem(T item)
        {
            var existing = this._collection.FirstOrDefault(i => i == item);
            if (existing == null)
                this._collection.Add(item);
            return item;
        }
        private T RemoveItem(T item)
        {
            this._collection.Remove(item);
            return item;
        }
    }
}
