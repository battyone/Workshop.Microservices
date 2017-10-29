using System.Threading.Tasks;
using NServiceBus.Logging;
using Divergent.Finance.Messages.Events;

namespace Divergent.Customers.Handlers
{
    public class PaymentSucceededHandler : NServiceBus.IHandleMessages<PaymentSucceededEvent>
    {
        private static readonly ILog Log = LogManager.GetLogger<PaymentSucceededHandler>();

        public async Task Handle(PaymentSucceededEvent message, NServiceBus.IMessageHandlerContext context)
        {
            Log.Info("Handling: PaymentSucceededEvent.");

        }
    }
}
