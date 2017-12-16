namespace DisTrace.Core
{
    public interface ITracingContextProvider
    {
        TracingContext GetTracingContext();
        void SetTracingContext(TracingContext tracingContext);
    }
}