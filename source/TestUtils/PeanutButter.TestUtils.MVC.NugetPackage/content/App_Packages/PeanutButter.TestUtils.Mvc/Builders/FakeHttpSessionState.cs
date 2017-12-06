using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace PeanutButter.TestUtils.MVC.Builders
{
    /// <summary>
    /// Original code: http://stephenwalther.com/archive/2008/07/01/asp-net-mvc-tip-12-faking-the-controller-context
    /// </summary>
    public class FakeHttpSessionState : HttpSessionStateBase
    {
        private readonly SessionStateItemCollection _sessionItems;
        public bool ClearWasCalled { get; set; }

        public FakeHttpSessionState(SessionStateItemCollection sessionItems)
        {
            _sessionItems = sessionItems;
        }


        public override void Add(string name, object value)
        {
            _sessionItems[name] = value;
        }

        public override void Clear()
        {
            ClearWasCalled = true;
            _sessionItems.Clear();
        }

        public override int Count => _sessionItems.Count;

        public override IEnumerator GetEnumerator()
        {
            return _sessionItems.GetEnumerator();
        }

        public override NameObjectCollectionBase.KeysCollection Keys => _sessionItems.Keys;

        public override object this[string name]
        {
            get
            {
                return _sessionItems[name];
            }
            set
            {
                _sessionItems[name] = value;
            }
        }

        public override int Timeout { get; set; }

        public override object this[int index]
        {
            get
            {
                return _sessionItems[index];
            }
            set
            {
                _sessionItems[index] = value;
            }
        }

        public override void Remove(string name)
        {
            _sessionItems.Remove(name);
        }
    }




}
