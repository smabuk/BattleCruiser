using System.Net;
using System.Net.Sockets;
using System.Text;

// 127.0.0.1 == localhost
// 0.0.0.0

Server server = new ();

await server.Start();