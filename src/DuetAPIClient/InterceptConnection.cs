using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DuetAPI;
using DuetAPI.Commands;
using DuetAPI.Connection;
using DuetAPI.Connection.InitMessages;
using DuetAPIClient.Exceptions;

namespace DuetAPIClient
{
    /// <summary>
    /// Connection class for intercepting G/M/T-codes from the control server
    /// </summary>
    /// <seealso cref="ConnectionMode.Intercept"/>
    public class InterceptConnection : BaseCommandConnection
    {
        /// <summary>
        /// Creates a new connection in intercepting mode
        /// </summary>
        public InterceptConnection() : base(ConnectionMode.Intercept) { }

        /// <summary>
        /// Mode of the interceptor
        /// </summary>
        public InterceptionMode Mode { get; private set; }

        /// <summary>
        /// Establishes a connection to the given UNIX socket file
        /// </summary>
        /// <param name="mode">Interception mode</param>
        /// <param name="socketPath">Path to the UNIX socket file</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Asynchronous task</returns>
        /// <exception cref="IncompatibleVersionException">API level is incompatible</exception>
        /// <exception cref="IOException">Connection mode is unavailable</exception>
        public Task Connect(InterceptionMode mode, string socketPath = Defaults.SocketPath, CancellationToken cancellationToken = default(CancellationToken))
        {
            InterceptInitMessage initMessage = new InterceptInitMessage { InterceptionMode = mode };
            Mode = mode;
            return Connect(initMessage, socketPath, cancellationToken);
        }

        /// <summary>
        /// Wait for a code to be intercepted and read it
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A code that can be intercepted</returns>
        public Task<Code> ReceiveCode(CancellationToken cancellationToken = default(CancellationToken)) => Receive<Code>(cancellationToken);

        /// <summary>
        /// Instruct the control server to ignore the last received code (in intercepting mode)
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <seealso cref="Ignore"/>
        /// <returns>Asynchronous task</returns>
        public Task IgnoreCode(CancellationToken cancellationToken = default(CancellationToken))
        {
            return PerformCommand(new Ignore(), cancellationToken);
        }

        /// <summary>
        /// Instruct the control server to resolve the last received code with the given message details (in intercepting mode)
        /// </summary>
        /// <param name="type">Type of the resolving message</param>
        /// <param name="content">Content of the resolving message</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Asynchronous task</returns>
        /// <seealso cref="Message"/>
        /// <seealso cref="Resolve"/>
        public Task ResolveCode(MessageType type, string content, CancellationToken cancellationToken = default(CancellationToken))
        {
            return PerformCommand(new Resolve { Content = content, Type = type }, cancellationToken);
        }
    }
}