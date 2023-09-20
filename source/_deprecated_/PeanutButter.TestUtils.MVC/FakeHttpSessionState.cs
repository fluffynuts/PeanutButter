using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace PeanutButter.TestUtils.MVC
{
    public class FakeHttpSessionState : HttpSessionStateBase
    {
        private readonly SessionStateItemCollection _sessionItems;
        public override int Timeout { get; set; }
        
        public override HttpSessionStateBase Contents => this;
        public override SessionStateMode Mode => SessionStateMode.Custom;
        public override int CodePage { get; set; }
        public override HttpCookieMode CookieMode => HttpCookieMode.AutoDetect;
        public override bool IsReadOnly => false;

        public FakeHttpSessionState()
        {
            _sessionItems = new SessionStateItemCollection();
        }

        public FakeHttpSessionState(SessionStateItemCollection sessionItems)
        {
            if (sessionItems == null)
            {
                _sessionItems = new SessionStateItemCollection();
            }
            else
            {
                _sessionItems = sessionItems;
            }
        }

        public override void Abandon()
        {
            _sessionItems.Clear();
        }

        public override void Clear()
        {
            _sessionItems.Clear();
        }

        public override void Add(string name, object value)
        {
            _sessionItems[name] = value;
        }

        public override int Count
        {
            get
            {
                return _sessionItems.Count;
            }
        }

        public override IEnumerator GetEnumerator()
        {
            return _sessionItems.GetEnumerator();
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return _sessionItems.Keys;
            }
        }

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
