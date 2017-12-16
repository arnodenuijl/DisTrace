namespace DisTrace.Core
{
    public static class TracingContextHeaders
    {
        public const string CausationIdHeaderName = "X-Causation-Id";
        public const string CorrelationIdHeaderName = "X-Correlation-Id";
        public const string RequestIdHeaderName = "X-Request-Id";
    }
}