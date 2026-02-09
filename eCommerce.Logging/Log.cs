using Microsoft.Extensions.Logging;

namespace eCommerce.Logging
{
    public static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "{Message}")]
        public static partial void Debug(
            ILogger logger, string message);
    }
}
