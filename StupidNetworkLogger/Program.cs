using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace NetworkLogger {
    class Program {
        static void Main(string[] args) {
            var server = new Server();
            Thread.Sleep(System.Threading.Timeout.Infinite);
        }
    }
}
