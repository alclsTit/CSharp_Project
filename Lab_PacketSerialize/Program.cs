using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ProtoBuf;

using Lab_PacketSerialize.Message;

namespace Lab_PacketSerialize
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Packet Serializing Performance Check");

            Stopwatch sw = new Stopwatch();

            DummyPacket packet = new DummyPacket();
            packet.logdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Console.Write("[Input times]:");
            var input = Console.ReadLine();

            Regex regex = new Regex("^[0-9]*$");
            var inputInt = Convert.ToInt32(input);
            if (inputInt <= 0 || !regex.IsMatch(input))
                return;

            sw.Start();
            for (var idx = 0; idx < inputInt; ++idx)
            {
                var resultBuffer = PacketParser.Instance.PacketToBuffer(packet);
                var resultPacket = PacketParser.Instance.BufferToPacket<DummyPacket>(new ArraySegment<byte>(resultBuffer.ToArray()));
                //Console.WriteLine($"[Size] = {resultPacket.size} / [Logdate] = {resultPacket.logdate}");
            }
            sw.Stop();

            Console.WriteLine($"[TryWriteBytes Performance] = [{input}] count / [{sw.Elapsed.TotalSeconds}] sec");


            sw.Start();
            for (var idx = 0; idx < inputInt; ++idx)
            {
                var resultBuffer = PacketParser.Instance.PacketToBuffer2(packet);
                var resultPacket = PacketParser.Instance.BufferToPacket2<DummyPacket>(new ArraySegment<byte>(resultBuffer.ToArray()));
                //Console.WriteLine($"[Size] = {resultPacket.size} / [Logdate] = {resultPacket.logdate}");
            }
            sw.Stop();

            Console.WriteLine($"[GetBytes Performance] = [{input}] count / [{sw.Elapsed.TotalSeconds}] sec");

        }
    }
}