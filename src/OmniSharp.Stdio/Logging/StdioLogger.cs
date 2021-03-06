using System;
using Microsoft.Extensions.Logging;
using OmniSharp.Stdio.Protocol;
using OmniSharp.Stdio.Services;

namespace OmniSharp.Stdio.Logging
{
    internal class StdioLogger : ILogger
    {
        private readonly object _lock = new object();
        private readonly ISharedTextWriter _writer;
        private readonly string _name;
        private readonly Func<string, LogLevel, bool> _filter;

        internal StdioLogger(ISharedTextWriter writer, string name, Func<string, LogLevel, bool> filter)
        {
            _writer = writer;
            _name = name;
            _filter = filter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter(this._name, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            string text = string.Empty;

            if (formatter != null)
            {
                text = formatter(state, exception);
            }
            else
            {
                if (state != null)
                {
                    text += state;
                }
                if (exception != null)
                {
                    text = text + Environment.NewLine + exception;
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var packet = new EventPacket()
            {
                Event = "log",
                Body = new
                {
                    LogLevel = logLevel.ToString().ToUpperInvariant(),
                    Name = this._name,
                    Message = text
                }
            };

            // don't block the logger
            _writer.WriteLineAsync(packet);
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
