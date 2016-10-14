using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using Newtonsoft.Json;
using SimpleHttpServer.Service;
using SocketLite.Services;

namespace HybridBridge.WebServer
{
    /// <summary>
    ///     LocalWebServer is an abstract class providing HTTP local server capacity and containing a list of registered
    ///     <see cref="IBridgeHandler" /> instances
    /// </summary>
    public abstract class HybridBridgeLocalServer : BridgeController, IDisposable
    {
        /// <summary>
        ///     Indicates if the object already disposed
        /// </summary>
        protected bool Disposed;

        /// <summary>
        ///     Creates a new instance of this class for passed collection of <see cref="IBridgeHandler" /> instances
        /// </summary>
        /// <param name="collection">The collection of <see cref="IBridgeHandler" /> instances to add to this list</param>
        protected HybridBridgeLocalServer(IEnumerable<IBridgeHandler> collection)
            : base(null, collection)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class
        /// </summary>
        protected HybridBridgeLocalServer() : this(null)
        {
        }

        /// <summary>
        ///     Underlying HttpListener instance
        /// </summary>
        public virtual HttpListener Server { get; protected set; }

        /// <summary>
        ///     The port number that the server listens to
        /// </summary>
        public virtual ushort PortNumber { get; private set; }

        /// <summary>
        ///     The method to dispose this class
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Makes sure that the passed custom protocol is acceptable and makes the necessarily changes
        /// </summary>
        protected override string NormalizeCustomProtocol(string customProtocol = null)
        {
            if (Protocol != null)
            {
                throw new NotSupportedException("Can not change the protocol string.");
            }
            return customProtocol != null ? new Uri(customProtocol).ToString().TrimEnd('/') + "/" : null;
        }

        /// <summary>
        ///     Starts the local HTTP server and listen for requests
        /// </summary>
        /// <returns>Task completion object</returns>
        /// <exception cref="InvalidOperationException">
        ///     If failed to find an unused port or prohibited from listening for new
        ///     connections.
        /// </exception>
        public virtual async Task RunServerAsync()
        {
            if (Server != null)
                return;
            const ushort validPortsMin = 1025;
            const ushort validPortsMax = ushort.MaxValue;
            var invalidPorts = new List<ushort>();
            var random = new Random();
            while (invalidPorts.Count < validPortsMax - validPortsMin)
            {
                PortNumber = (ushort) random.Next(validPortsMin, validPortsMax);
                if (invalidPorts.Contains(PortNumber))
                    continue;
                try
                {
                    Server = new HttpListener(TimeSpan.FromSeconds(30));
                    await Server.StartTcpRequestListener(PortNumber);
                    Server.HttpRequestObservable.Subscribe(OnHttpRequest);
                    using (var testSocket = new TcpSocketClient())
                    {
                        var tokenSource = new CancellationTokenSource();
                        var connectOperation = testSocket.ConnectAsync("127.0.0.1", PortNumber.ToString(), false,
                            tokenSource.Token);
                        tokenSource.CancelAfter(TimeSpan.FromSeconds(10));
                        await connectOperation;
                        if (connectOperation.IsFaulted || connectOperation.IsCanceled)
                        {
                            if (connectOperation.Exception != null)
                                throw connectOperation.Exception;
                            throw new InvalidOperationException();
                        }
                        testSocket.Disconnect();
                    }
                    Protocol = @"http://127.0.0.1:" + PortNumber;
                }
                catch (NotImplementedException)
                {
                    Server = null;
                    throw;
                }
                catch
                {
                    invalidPorts.Add(PortNumber);
                    try
                    {
                        Server?.StopTcpRequestListener();
                    }
                    catch
                    {
                        // ignored
                    }
                    Server = null;
                    continue;
                }
                return;
            }
            throw new InvalidOperationException(@"There is no free port to bind to on this machine.");
        }

        /// <summary>
        ///     Analyzes and responses to new HTTP requests received by the server
        /// </summary>
        /// <param name="httpRequest">The <see cref="IHttpRequest" /> implemented object containing information about the request</param>
        protected virtual async void OnHttpRequest(IHttpRequest httpRequest)
        {
            if (httpRequest.Method == null)
            {
                return;
            }
            var httpResponse = new HttpResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                ResponseReason = HttpStatusCode.OK.ToString(),
                Headers = new Dictionary<string, string>
                {
                    {@"Date", DateTime.UtcNow.ToString("r")},
                    {@"Content-Type", @"application/json; charset=UTF-8"},
                    {@"Access-Control-Allow-Origin", @"*"},
                    {@"Access-Control-Allow-Headers", @"Content-Type"}
                }
            };
            bool hasResult;
            object result;
            if ((httpRequest.Method == "GET") && (httpRequest.Path != null) &&
                HandleRequestUrl(Protocol.TrimEnd('/') + httpRequest.RequestUri, out hasResult, out result))
            {
                httpResponse.Body =
                    new MemoryStream(Encoding.UTF8.GetBytes(hasResult ? JsonConvert.SerializeObject(result) : ""));
            }
            await Server.HttpReponse(httpRequest, httpResponse);
        }

        /// <summary>
        ///     The method to dispose this class
        /// </summary>
        ~HybridBridgeLocalServer()
        {
            Dispose(false);
        }

        /// <summary>
        ///     The method to dispose this class
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                try
                {
                    Server?.StopTcpRequestListener();
                }
                catch
                {
                    // ignored
                }
                Server = null;
            }

            Disposed = true;
        }
    }
}