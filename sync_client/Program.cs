using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace sync_client
{
    class Program
    {
        static void Main(string[] args)
        {


            
            
            Console.WriteLine("Hello World!");
            TcpClient client = new TcpClient("127.0.0.1", 8001);
            try
            {
                Stream s = client.GetStream();
                StreamReader sr = new StreamReader(s);
                StreamWriter sw = new StreamWriter(s);
                sw.AutoFlush = true;
                Console.WriteLine(sr.ReadLine());
                while (true)
                {
                    Console.Write("Name: ");
                    string name = Console.ReadLine();
                    sw.WriteLine(name);
                    if (name == "") break;
                    Console.WriteLine(sr.ReadLine());
                }
                s.Close();
            }
            finally
            {
                // code in finally block is guranteed 
                // to execute irrespective of 
                // whether any exception occurs or does 
                // not occur in the try block
                client.Close();
            }
        }
    }
}
