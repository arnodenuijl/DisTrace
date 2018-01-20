using System;

namespace DisTrace.Core
{
    public class TracingContext
    {
        public TracingContext(string unitOfWorkId, string causationId, string flowId)
        {
            UnitOfWorkId = !string.IsNullOrWhiteSpace(unitOfWorkId)
                ? unitOfWorkId
                : Guid.NewGuid().ToString();

            CausationId = !string.IsNullOrWhiteSpace(causationId)
                ? causationId
                : null;

            FlowId = !string.IsNullOrWhiteSpace(flowId)
                ? flowId
                : CausationId ?? UnitOfWorkId;
        }

        public string UnitOfWorkId { get; }
        public string CausationId { get; }
        public string FlowId { get; }

        public static TracingContext CreateFromUnitOfWorkId(string unitOfWorkId)
        {
            if (string.IsNullOrWhiteSpace(unitOfWorkId))
            {
                throw new ArgumentException($"{nameof(unitOfWorkId)} connot be null or empty string", nameof(unitOfWorkId));
            }

            return new TracingContext(unitOfWorkId, null, unitOfWorkId);
        }        
    }
}