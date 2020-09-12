using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Client  socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}
public enum SystemStatus
{ 
    NoCammand,
    IsSending,
    Paused,
    Stoped
}
public class AsynchronousSocketListener
{
    private static int _portNumber = 8096;
    private static string _hostIp = "127.0.0.1";
    private static RNGCryptoServiceProvider Rand = new RNGCryptoServiceProvider();
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    private static ManualResetEvent sendData = new ManualResetEvent(false);
    //server state
    static double _startStep = 0;
    static bool _sendData = false;
    static bool _sendingStarted = false;
    static SystemStatus _systemStatus = SystemStatus.NoCammand;
    static bool _instanceIsCreated = false;
    public AsynchronousSocketListener()
    {
    }

    public static void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPAddress ipAddress = IPAddress.Parse(_hostIp);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _portNumber);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.  
                allDone.Reset();
                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a connection...");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();
            if (content.IndexOf("\r") > -1)
            {
                // All the data has been read from the
                // client. Display it on the console.  
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                switch (content.Replace("\r", ""))
                {
                    case "stop":
                        state.sb.Clear();
                        Stop();
                        return;
                    case "pause":
                        state.sb.Clear();
                        Pause();
                        //for avoiding adding new instance must return
                        return;
                    case "start":
                        //if there is active instance we must return to avoiding new instance
                        if (Start()) 
                            return;
                        state.sb.Clear();
                        break;
                    default:
                        return;
                }
                
                Stopwatch stopWatch = new Stopwatch();
                // Echo the data back to the client.  
                stopWatch.Start();
                double stepsCount = 5000;
                int counter = 0;
                if (_sendData)
                {
                    for (double xStep = 0; xStep < 14;)
                    {
                        xStep = _startStep;
                        string str = "!1\t" + GenerateFakeLoad(xStep).ToString() + "\r";
                        _startStep = xStep += ((double)20 / (double)stepsCount);
                        counter++;
                        state.sb.Clear();
                        Send(handler, str);
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                new AsyncCallback(ReadCallback), state);
                        Console.WriteLine("{1}: Sent: {0}", str, Thread.CurrentThread.ManagedThreadId);
                        Thread.Sleep(TimeSpan.Zero);
                    }
                    _systemStatus = SystemStatus.NoCammand;
                    
                    stopWatch.Stop();
                    // Get the elapsed time as a TimeSpan value.
                    TimeSpan ts = stopWatch.Elapsed;
                    // Format and display the TimeSpan value.
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                    Console.WriteLine("RunTime " + elapsedTime);
                    Console.WriteLine(counter + " message is send!");
                }
                else
                {
                    state.sb.Clear();
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);

                }
            }
            else
            {
                state.sb.Clear();
                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
        }
    }

    private static void Send(Socket handler, String data)
    {
        sendData.WaitOne();
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            sendData.WaitOne();
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            //Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    private static int GenerateRandomNumber(int min, int max)
    {
        uint scale = uint.MaxValue;
        while (scale == uint.MaxValue)
        {
            // Get four random bytes.
            byte[] four_bytes = new byte[4];
            Rand.GetBytes(four_bytes);

            // Convert that into an uint.
            scale = BitConverter.ToUInt32(four_bytes, 0);
        }

        // Add min to the scaled difference between max and min.
        return (int)(min + (max - min) *
            (scale / (double)uint.MaxValue));
    }
    private static double GenerateFakeLoad(double xStep)
    {
        double fakeLoad = 0;
        if (xStep <= 10)
        {
            fakeLoad = ((-1 * (xStep) * (xStep)) / 10) + (2 * xStep);
        }
        else if (xStep > 10)
        {
            fakeLoad = (-1 * (xStep) * (xStep)) + (20 * xStep) - 90;
        }
        else 
        {
            fakeLoad = 0;
        }
        return fakeLoad;
    }
    private static bool Start()
    {
        if (_systemStatus == SystemStatus.IsSending)
        {
            return true;
        }
        else if (_systemStatus == SystemStatus.NoCammand)
        {
            _startStep = 0;
            _sendData = true;
            sendData.Set();
            _systemStatus = SystemStatus.IsSending;
            return false;
        }
        else if (_systemStatus == SystemStatus.Stoped)
        {
            _startStep = 0;
            _sendData = true;
            sendData.Set();
            return true;
        }
        else if (_systemStatus == SystemStatus.Paused)
        {
            _sendData = true;
            sendData.Set();
            return true;
        }
        return true;
    }
    private static void Pause()
    {
        if (_systemStatus == SystemStatus.Paused || _systemStatus == SystemStatus.NoCammand) return;
        _sendData = false;
        _systemStatus = SystemStatus.Paused;
        sendData.Reset();
    }
    private static void Stop()
    {
        if (_systemStatus == SystemStatus.Stoped || _systemStatus == SystemStatus.NoCammand) return;
        _sendData = false;
        _startStep = 0;
        _systemStatus = SystemStatus.Stoped;
        sendData.Reset();
    }
    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }
}
