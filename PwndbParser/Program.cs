using System;
using System.Net;
using System.Net.Http;
using CommandDotNet;
using Knapcode.TorSharp;

namespace PwndbParser
{
    class Program
    {
        static int Main(string[] args)
        {
            return new AppRunner<CommandParser>().Run(args);
        }
    }
}
