using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;


namespace SOS.Lib.Helpers
{
    public static class RedisSentinelHelper
    {
        public static ConnectionMultiplexer ConnectToRedisViaSentinel(string sentinelHost, int sentinelPort, string masterName, string redisPassword)
        {
            var sentinel = new IPEndPoint(Dns.GetHostAddresses(sentinelHost).First(), sentinelPort);
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(sentinel);

            using var networkStream = new NetworkStream(socket);
            using var writer = new StreamWriter(networkStream) { AutoFlush = true };
            using var reader = new StreamReader(networkStream);

            // Send SENTINEL get-master-addr-by-name <master-name>
            writer.WriteLine($"*3\r\n$8\r\nSENTINEL\r\n$23\r\nget-master-addr-by-name\r\n${masterName.Length}\r\n{masterName}\r\n");

            // Read the response (simple protocol parsing)
            var line = reader.ReadLine();
            if (line != "*2")
                throw new Exception("Unexpected Sentinel response.");

            var ip = reader.ReadLine(); // length line
            ip = reader.ReadLine();     // actual IP

            var portLine = reader.ReadLine(); // length
            var port = reader.ReadLine();     // actual port

            var masterEndpoint = $"{ip}:{port}";

            var options = ConfigurationOptions.Parse(masterEndpoint);
            options.Password = redisPassword;
            options.AllowAdmin = true;

            return ConnectionMultiplexer.Connect(options);
        }
    }
}
