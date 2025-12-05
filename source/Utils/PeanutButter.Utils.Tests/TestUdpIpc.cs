using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using PeanutButter.Utils.Experimental;

#pragma warning disable 618
namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestUdpIpc
    {
        [TestFixture]
        public class IpcComms
        {
            [Test]
            [Ignore("WIP")]
            public void SendAndReceive()
            {
                // Arrange
                var identifier = GetRandomString(1);
                using (var peer1 = new UdpIpc(identifier))
                using (var peer2 = new UdpIpc(identifier))
                {
                    // Act
                    // Assert
                }
            }
        }

        [TestFixture]
        public class Messages
        {
            [DataContract]
            public class DiscoveryPayload
            {
                [DataMember]
                public int ListenPort { get; set; }
            }

            [Test]
            [Explicit("Discovery")]
            public void ShouldBeAbleToSerializeAndDeserialize()
            {
                // Arrange
                var message = Message.For(new DiscoveryPayload() { ListenPort = 1234 });
                var serializer = new XmlSerializer(message.GetType());
                var baseSerializer = new XmlSerializer(typeof(Message));
                var stream = new MemoryStream();
                // Act
                serializer.Serialize(stream, message);
                var serialized = stream.AsString();
                stream.Position = 0;
                
                // this is how to get the name of the type implied by the message
                // -> we should require consumers to register payload types to avoid
                //    crazy shit like scanning all assemblies (and having to deal with
                //    conflicts!)
                var doc = XDocument.Parse(serialized);
                var typeElement = doc.XPathSelectElements("//Type").FirstOrDefault();
                var messageType = typeElement.Value;
                
                var genericType = typeof(Message<>);
                var payloadType = typeof(DiscoveryPayload);
                var specificType = genericType.MakeGenericType(payloadType);
                var deserializer = new XmlSerializer(specificType);
                stream.Position = 0;
                var deserialized = deserializer.Deserialize(stream);
                // Assert
                Console.WriteLine(serialized);
            }
        }
    }
}
#pragma warning restore 618
