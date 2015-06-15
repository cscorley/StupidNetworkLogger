using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Data.SQLite;
using System.Diagnostics;

namespace NetworkLogger {
    class Server {
        private TcpListener tcpListener;
        private Thread listenThread;
        private string loc = "Data Source=logger.s3db";
        private SQLiteConnection conn;

        public Server() {
            this.tcpListener = new TcpListener(IPAddress.Any, 9999);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
            conn = new SQLiteConnection(loc).OpenAndReturn();
            var table = "create table if not exists log (recv integer, message text);";
            Debug.WriteLine("Exec: " + table);
            SQLiteCommand cmd = new SQLiteCommand(table, conn);
            cmd.ExecuteNonQueryAsync();
        }

        private void Add(string msg) {
            var text = new StringBuilder();
            text.Append("insert into log (recv, message) values (");
            text.Append(DateTime.UtcNow.Ticks);
            text.Append(", \"");
            text.Append(msg);
            text.Append("\");");
            Debug.WriteLine("Exec: " + text.ToString());

            SQLiteCommand cmd = new SQLiteCommand(text.ToString(), conn);
            cmd.ExecuteNonQueryAsync();
        }

        private void ListenForClients() {
            this.tcpListener.Start();

            while (true) {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }
        private void HandleClientComm(object client) {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true) {
                bytesRead = 0;

                try {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0) {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                UTF8Encoding encoder = new UTF8Encoding();
                string text = encoder.GetString(message, 0, bytesRead);
                using (var reader = new StringReader(text)) {
                    string line = reader.ReadLine();
                    if (line != null) {
                        Add(line);
                    }
                }

            }

            tcpClient.Close();
        }
    }
}
