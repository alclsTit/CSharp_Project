using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using Google.Protobuf.Protocol.Session;

using static Google.Protobuf.Examples.AddressBook.Person.Types;

namespace Lab_EchoServerEngine
{
    public class Class1
    {
        public Class1()
        {
            notify_session_connect_server2game notify_session_connect = new notify_session_connect_server2game
            {
                Header = new Google.Protobuf.Protocol.MessageHeader.MessageHeaderNetwork 
                { 
                    MsgId = Google.Protobuf.Protocol.MessageHeader.eNetworkMessageId.NotifySessionConnect, 
                    MsgHeader = 
                    {
                        MsgVersion = 1, 
                        SeqNumber = 1 
                    } 
                },
                SessionId = "1",
                LoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            Person person = new Person
            {
                Id = 1234,
                Name = "John Doe",
                Email = "jdoe@example.com",
                Phones = { new PhoneNumber { Number = "555-4321", Type = PhoneType.Home } }
            };

            Person person2 = new Person
            {
                Id = 5678,
                Name = "Steve Rock",
                Email = "srock@example.com",
                Phones = { new PhoneNumber { Number = "172-3829", Type = PhoneType.Mobile } }
            };

            byte[] bytes, bytes2, bytes3;
            using (MemoryStream ms = new MemoryStream())
            {
                // 직렬화
                person.WriteTo(ms);
                bytes = ms.ToArray();
            }

            using(MemoryStream ms2 = new MemoryStream())
            {
                person2.WriteTo(ms2);
                bytes2 = ms2.ToArray();
            }
            
            using(MemoryStream ms3 = new MemoryStream())
            {
                notify_session_connect.WriteTo(ms3);
                bytes3 = ms3.ToArray();
            }

            // 역직렬화
            Person copy = Person.Parser.ParseFrom(bytes);
            Person copy2 = Person.Parser.ParseFrom(bytes2);
            notify_session_connect_server2game copy3 = notify_session_connect_server2game.Parser.ParseFrom(bytes3);

            AddressBook book = new AddressBook
            {
                People = { copy, copy2 }
            };

            bytes = book.ToByteArray();
            bytes2 = book.ToByteArray();

            AddressBook restored = AddressBook.Parser.ParseFrom(bytes);

            Console.WriteLine($"Message => {copy3.Header.MsgId} / {copy3.Header.MsgHeader.SeqNumber} / {copy3.Header.MsgHeader.MsgVersion} / {copy3.SessionId} / {copy3.LoginDate}");

            if (restored.People.Count != 1 || !person.Equals(restored.People[0]))
            {
                throw new Exception("There is a bad person in here!");
            }
            
        }    
    }
}