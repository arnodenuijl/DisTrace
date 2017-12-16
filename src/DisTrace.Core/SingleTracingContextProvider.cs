namespace DisTrace.Core
{
    /// <summary>
    ///     Holds a single tracingContext.
    ///     Usable for simple cases where
    /// </summary>
    public class SingleTracingContextProvider : ITracingContextProvider
    {
        private TracingContext _tracingContext;

        public TracingContext GetTracingContext()
        {
            return _tracingContext;
        }

        public void SetTracingContext(TracingContext tracingContext)
        {
            _tracingContext = tracingContext;
        }
    }
}