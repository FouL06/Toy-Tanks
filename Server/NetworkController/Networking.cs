///<summary>
/// Author: Ashton Foulger & Austin In, CS 3500 - 001 Fall 2021
/// Version: 0.2 - (11/8/21)
///</summary>

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil
{
    /// <summary>
    /// Netcode Controller allowing users to connect to an IP or URL with proper ports.
    /// Allowing for connection to a server or chat client.
    /// </summary>
    public static class Networking
    {
        /////////////////////////////////////////////////////////////////////////////////////////
        // Server-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
        /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
        /// AcceptNewClient will continue the event-loop.
        /// </summary>
        /// <param name="toCall">The method to call when a new connection is made</param>
        /// <param name="port">The the port to listen on</param>
        public static TcpListener StartServer(Action<SocketState> toCall, int port)
        {
            try
            {
                // initialize the listener
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                // create socket for server and begin accepting clients
                Tuple<Action<SocketState>, TcpListener> tuple = new Tuple<Action<SocketState>, TcpListener>(toCall, listener);
                listener.BeginAcceptSocket(AcceptNewClient, tuple);
                return listener;
            }
            catch (Exception)
            {
                // if server fails to start/initialize, throw error message
                string Error_Message = "Failed to start server...";
                SocketState Error_State = SocketStateErrorConfiguration(Error_Message, toCall);
                Error_State.OnNetworkAction(Error_State);
                return null;
            }
        }

        /// <summary>
        /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
        /// continues an event-loop to accept additional clients.
        ///
        /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
        /// OnNetworkAction should be set to the delegate that was passed to StartServer.
        /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
        /// 
        /// If anything goes wrong during the connection process (such as the server being stopped externally), 
        /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
        /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
        /// an error occurs.
        ///
        /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
        /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
        /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
        private static void AcceptNewClient(IAsyncResult ar)
        {
            //Initialize AsyncResult and TcpListener
            Tuple<Action<SocketState>, TcpListener> tuple = (Tuple<Action<SocketState>, TcpListener>)ar.AsyncState;
            Action<SocketState> toCall = tuple.Item1;
            TcpListener listener = tuple.Item2;

            try
            {
                // initialize socket and socket state
                Socket socket = listener.EndAcceptSocket(ar);
                SocketState state = new SocketState(toCall, socket);
                state.OnNetworkAction(state);

                // begin accepting new client connections
                listener.BeginAcceptSocket(AcceptNewClient, tuple);
            }
            catch (Exception)
            {
                // if server is full or server is closed, don't accept any more clients
                string Error_Message = "Not accepting new client connections or server is closing...";
                SocketState Error_State = SocketStateErrorConfiguration(Error_Message, toCall);
                Error_State.OnNetworkAction(Error_State);
            }
        }

        /// <summary>
        /// Stops the given TcpListener.
        /// </summary>
        public static void StopServer(TcpListener listener)
        {
            listener.Stop();
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Client-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of connecting to a server via BeginConnect, 
        /// and using ConnectedCallback as the method to finalize the connection once it's made.
        /// 
        /// If anything goes wrong during the connection process, toCall should be invoked 
        /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
        /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
        /// in this method or in ConnectedCallback.
        ///
        /// This connection process should timeout and produce an error (as discussed above) 
        /// if a connection can't be established within 3 seconds of starting BeginConnect.
        /// 
        /// </summary>
        /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
        /// <param name="hostName">The server to connect to</param>
        /// <param name="port">The port on which the server is listening</param>
        public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
        {
            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo;
            IPAddress ipAddress = IPAddress.None;

            // Determine if the server address is a URL or an IP
            try
            {
                ipHostInfo = Dns.GetHostEntry(hostName);
                bool foundIPV4 = false;
                foreach (IPAddress addr in ipHostInfo.AddressList)
                    if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        foundIPV4 = true;
                        ipAddress = addr;
                        break;
                    }
                // Didn't find any IPV4 addresses
                if (!foundIPV4)
                {
                    string Error_Message = "IPV4 address could not be found...";
                    SocketState Error_State = SocketStateErrorConfiguration(Error_Message, toCall);
                    Error_State.OnNetworkAction(Error_State);
                }
            }
            catch (Exception)
            {
                // see if host name is a valid ip-address
                try
                {
                    ipAddress = IPAddress.Parse(hostName);
                }
                catch (Exception) //Send user error message if IP address does not exsist
                {
                    string Error_Message = "Hostname IP address could not be found or does not exsist...";
                    SocketState Error_State = SocketStateErrorConfiguration(Error_Message, toCall);
                    Error_State.OnNetworkAction(Error_State);
                }
            }

            // Create a TCP/IP socket.
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // This disables Nagle's algorithm (google if curious!)
            // Nagle's algorithm can cause problems for a latency-sensitive 
            // game like ours will be 
            socket.NoDelay = true;

            try
            {
                //Create New SocketState and establish the connection
                SocketState state = new SocketState(toCall, socket);
                Tuple<Action<SocketState>, Socket> tuple = new Tuple<Action<SocketState>, Socket>(toCall, state.TheSocket);
                IAsyncResult result = state.TheSocket.BeginConnect(ipAddress, port, ConnectedCallback, tuple);

                //Connection Timeout Check
                result.AsyncWaitHandle.WaitOne(5000);
                if (!socket.Connected)
                {
                    string Error_Message = "Connection timeout occured...";
                    SocketState Error_State = SocketStateErrorConfiguration(Error_Message, toCall);
                    Error_State.OnNetworkAction(Error_State);
                }
            }
            catch (Exception) //If all connection attempts fail throw an error
            {
                string Error_Message = "Failed to connect to server IP/Hostname address...";
                SocketState Error_State = SocketStateErrorConfiguration(Error_Message, toCall);
                Error_State.OnNetworkAction(Error_State);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
        ///
        /// Uses EndConnect to finalize the connection.
        /// 
        /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
        /// either this method or ConnectToServer should indicate the error appropriately.
        /// 
        /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
        /// with a new SocketState representing the new connection.
        /// 
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginConnect</param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            //Initalize AsyncResult and set the Socket and SocketState
            Tuple<Action<SocketState>, Socket> tuple = (Tuple<Action<SocketState>, Socket>)ar.AsyncState;
            Action<SocketState> toCall = tuple.Item1;
            Socket socket = tuple.Item2;

            try
            {
                //EndConnection Call and check SocketState
                socket.EndConnect(ar);
                SocketState Good_Connection_State = new SocketState(toCall, socket);
                Good_Connection_State.OnNetworkAction(Good_Connection_State);

                //Let the user know that the connection was made
                Console.WriteLine("Connection was successfully made with the host...");
            }
            catch (Exception)
            {
                //Error is left empty since the error message is handled in the ConnectToServer method
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Server and Client Common Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
        /// as the callback to finalize the receive and store data once it has arrived.
        /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
        /// 
        /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
        /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
        /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
        /// in this method or in ReceiveCallback.
        /// </summary>
        /// <param name="state">The SocketState to begin receiving</param>
        public static void GetData(SocketState state)
        {
            try
            {
                //Begin receiving data from connection
                state.TheSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception) //If connection is lost or packets are not recieved throw error
            {
                string Error_Message = "Failed to receive data from connection...";
                state.ErrorOccurred = true;
                state.ErrorMessage = Error_Message;
                state.OnNetworkAction(state);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
        /// 
        /// Uses EndReceive to finalize the receive.
        ///
        /// As stated in the GetData documentation, if an error occurs during the receive process,
        /// either this method or GetData should indicate the error appropriately.
        /// 
        /// If data is successfully received:
        ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
        ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its 
        ///      string builder.
        ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
        /// </summary>
        /// <param name="ar"> 
        /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
        /// </param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            //Initalize the socket state
            SocketState state = (SocketState)ar.AsyncState;

            try
            {
                //Get Data Packets
                int packets = state.TheSocket.EndReceive(ar);

                //Check if no packets are recieved
                if (packets == 0)
                {
                    string Error_Message = "Connection failed to send data closing connection to server...";
                    state.ErrorOccurred = true;
                    state.ErrorMessage = Error_Message;
                    state.OnNetworkAction(state);
                    return;
                }

                //Threading to read the socket connection is packet data
                lock (state)
                {
                    //Get packets data and add it to unproccessed data for the socket to read
                    string data = Encoding.UTF8.GetString(state.buffer, 0, packets);
                    state.data.Append(data);
                    state.OnNetworkAction(state);
                }
            }
            catch (Exception) //If a packet could not be read or failed to be recieved throw an error and lock the thread to prevent data from being read
            {
                lock (state)
                {
                    string Error_Message = "Failed to receive data packet from connection...";
                    state.ErrorOccurred = true;
                    state.ErrorMessage = Error_Message;
                    state.OnNetworkAction(state);
                }
            }
        }

        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool Send(Socket socket, string data)
        {
            try
            {
                // if socket is open, send data
                if (socket.Connected)
                {
                    byte[] packets = Encoding.UTF8.GetBytes(data);
                    socket.BeginSend(packets, 0, packets.Length, SocketFlags.None, SendCallback, socket);
                    return true;
                }
                else // if socket is closed, ensure that it's closed
                {
                    socket.Close();
                    return false;
                }
            }
            catch (Exception) // if socket is closed for any reason, ensure that the socket is closed
            {
                socket.Close();
                return false;
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by Send.
        ///
        /// Uses EndSend to finalize the send.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndSend(ar);
            }
            catch (Exception) // if an error occurred while sending, ensure the socket is closed.
            {
                socket.Close();
            }
        }

        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
        /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool SendAndClose(Socket socket, string data)
        {
            return Send(socket, data); //Send method will ensure that the connection is closed, and finalize the sending process.
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
        ///
        /// Uses EndSend to finalize the send, then closes the socket.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendAndCloseCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndSend(ar);
            }
            catch (Exception) // if error occurred while sending, ensure the socket is closed.
            {
                socket.Close();
            }
            finally // ensure the socket is closed before returning
            {
                socket.Close();
            }
        }

        /// <summary>
        /// Creates and error message from the errorCall socket accessing the SocketState API to throw errors,
        /// when and error occur.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="errorCall"></param>
        /// <returns></returns>
        private static SocketState SocketStateErrorConfiguration(string error, Action<SocketState> errorCall)
        {
            //Create new socket for when an error occures and get error message.
            SocketState Error_State = new SocketState(errorCall, null);
            Error_State.ErrorOccurred = true;
            Error_State.ErrorMessage = error;
            return Error_State;
        }
    }
}