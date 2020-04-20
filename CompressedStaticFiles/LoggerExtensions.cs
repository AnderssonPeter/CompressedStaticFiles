using Microsoft.Extensions.Logging;
using System;

namespace CompressedStaticFiles
{
    internal static class LoggerExtensions
    {
        private static Action<ILogger, string, string, long, long, Exception> _logFileServed;

        static LoggerExtensions()
        {
            _logFileServed = LoggerMessage.Define<string, string, long, long>(
               logLevel: LogLevel.Information,
               eventId: 1,
               formatString: "Sending file. Request file: '{RequestedPath}'. Served file: '{ServedPath}'. Original file size: {OriginalFileSize}. Served file size: {ServedFileSize}");
        }

        public static void LogFileServed(this ILogger logger, string requestedPath, string servedPath, long originalFileSize, long servedFileSize)
        {
            if (string.IsNullOrEmpty(requestedPath))
            {
                throw new ArgumentNullException(nameof(requestedPath));
            }
            if (string.IsNullOrEmpty(servedPath))
            {
                throw new ArgumentNullException(nameof(servedPath));
            }
            _logFileServed(logger, requestedPath, servedPath, originalFileSize, servedFileSize, null);
        }
    }
}
