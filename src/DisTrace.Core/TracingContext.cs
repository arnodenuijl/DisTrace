namespace DisTrace.Core
{
    public class TracingContext
    {
        public TracingContext(string requestId, string causationId, string correlationId)
        {
            RequestId = requestId;
            CausationId = causationId;
            CorrelationId = correlationId;
        }

        public string RequestId { get; }
        public string CausationId { get; }
        public string CorrelationId { get; }

        public static TracingContext CreateNew(string requestId)
        {
            return new TracingContext(requestId, null, requestId);
        }
    }
}