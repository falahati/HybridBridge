using System.Collections.Generic;
using System.IO;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace HybridBridge.WebServer
{
    internal class HttpResponse : IHttpResponse
    {
        public RequestType RequestType { get; set; }
        public ITcpSocketClient TcpSocketClient { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public MemoryStream Body { get; set; }
        public int RemotePort { get; set; }
        public string RemoteAddress { get; set; }
        public bool IsEndOfRequest { get; set; }
        public bool IsRequestTimedOut { get; set; }
        public bool IsUnableToParseHttp { get; set; }
        public int StatusCode { get; set; }
        public string ResponseReason { get; set; }
    }
}