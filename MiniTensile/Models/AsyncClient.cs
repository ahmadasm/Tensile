using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.IO;

namespace MiniTensile.Models
{
    
    public class AsyncClient
    {
        // The port number for the remote device.  
        private  int _portNumber = 8096;
        private string _hostIp = "127.0.0.1";
        private static Socket _client;
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);
        public delegate void MessageReceivedHandler(object sender,string message);
        public event MessageReceivedHandler MessageReceived;
        // The response from the remote device.  
        //private static String response = String.Empty;

        public AsyncClient() { }

        public AsyncClient(string hostIp, int portNumber)
        {
            this._hostIp = hostIp;
            this._portNumber = portNumber;
        }
        public bool IsConnected { get => _client.Connected; }
        public void Connect()
        {
            // Connect to a remote device.  
            try
            {
                IPAddress ipAddress = IPAddress.Parse(_hostIp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _portNumber);

                // Create a TCP/IP socket.  
                _client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), _client);
                connectDone.WaitOne();

                // Send test data to the remote device.  
                //Send("This is a test<EOF>");
                //sendDone.WaitOne();

                // Receive the response from the remote device.  
                //Receive();
                //receiveDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void Disconnect()
        {
            // Release the socket.  
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }
        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            _client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), _client);
            sendDone.WaitOne();
            Receive(_client);
            receiveDone.WaitOne();
        }
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                _client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        protected virtual void RaiseMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    string content = state.sb.ToString();
                    if ((content.IndexOf("\r") > -1) || (content.IndexOf("\n") > -1) || (content.IndexOf("\r\n") > -1))
                    {
                        // Signal that all bytes have been received.  
                        state.sb.Clear();
                        receiveDone.Set();
                        RaiseMessageReceived(content);                        
                    }

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                      new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
