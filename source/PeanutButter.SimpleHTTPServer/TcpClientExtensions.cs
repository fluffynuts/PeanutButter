using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PeanutButter.SimpleHTTPServer
{
    public static class TcpClientExtensions
    {
        public static string ReadLine(this TcpClient client)
        {
            var stream = client.GetStream();
            var data = new List<char>();
            while (true) 
            {
                var thisChar = stream.ReadByte();
                if (thisChar == '\n') break;
                if (thisChar == '\r') continue;
                if (thisChar < 0) 
                { 
                    Thread.Sleep(0); 
                    continue; 
                }
                data.Add(Convert.ToChar(thisChar));
            }            
            return string.Join(string.Empty, data);
        }
    }
}