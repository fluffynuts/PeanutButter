using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

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
                
                var re = new Regex("(?:</([a-zA-Z0-9]+)>)$");
                var matches = re.Matches(serialized);

                var baseMessage = (Message)baseSerializer.Deserialize(stream);
                var genericType = typeof(Message<>);
                var payloadType = genericType.Assembly.FindTypeByName(baseMessage.Type);
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