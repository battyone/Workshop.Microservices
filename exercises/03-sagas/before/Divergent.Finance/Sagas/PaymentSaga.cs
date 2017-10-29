using System;
using System.Threading.Tasks;
using Divergent.Finance.Messages;
using Divergent.Finance.Messages.Commands;
using Divergent.Finance.Messages.Events;
using NServiceBus;

namespace Divergent.Finance.Sagas
{
    public class PaymentSaga : Saga<PaymentSaga.PaymentSagaData>, IAmStartedByMessages<InitiatePaymentProcessCommand>,
        IAmStartedByMessages<PaymentTimedOutMessage>, IAmStartedByMessages<PaymentSucceededEvent>
    {
        public async Task Handle(InitiatePaymentProcessCommand message, IMessageHandlerContext context)
        {
            Data.OrderId = message.OrderId;
            Data.IsPaymentInitiated = true;
            Data.Amount = message.Amount;
            Data.CustomerId = message.CustomerId;

            await SendProcessPayment(context);

            var sendOptions = new SendOptions();
            sendOptions.DelayDeliveryWith(TimeSpan.FromSeconds(30));
            await context.Send(new PaymentTimedOutMessage {OrderId = Data.OrderId}, sendOptions);
        }

        public Task Handle(PaymentSucceededEvent message, IMessageHandlerContext context)
        {
            Data.OrderId = message.OrderId;
            Data.IsPaymentSucceeded = true;
            return Task.CompletedTask;
        }

        public async Task Handle(PaymentTimedOutMessage message, IMessageHandlerContext context)
        {
            if (Data.RetryAttempts > 3)
            {
                // TODO: send failed message
            }
            else
            {
                Data.RetryAttempts++;
                await SendProcessPayment(context);
            }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PaymentSagaData> mapper)
        {
            mapper.ConfigureMapping<InitiatePaymentProcessCommand>(p => p.OrderId).ToSaga(s => s.OrderId);
            mapper.ConfigureMapping<PaymentSucceededEvent>(p => p.OrderId).ToSaga(s => s.OrderId);
            mapper.ConfigureMapping<PaymentTimedOutMessage>(p => p.OrderId).ToSaga(s => s.OrderId);
        }

        private async Task SendProcessPayment(IMessageHandlerContext context)
        {
            await context.Send(new ProcessPaymentMessage
            {
                OrderId = Data.OrderId,
                Amount = Data.Amount,
                CustomerId = Data.CustomerId
            });
        }

        public class PaymentSagaData : ContainSagaData
        {
            public virtual int OrderId { get; set; }
            public virtual bool IsPaymentInitiated { get; set; }
            public virtual bool IsPaymentSucceeded { get; set; }
            public virtual int RetryAttempts { get; set; }
            public virtual double Amount { get; set; }
            public virtual int CustomerId { get; set; }
        }
    }

    public class PaymentTimedOutMessage
    {
        public int OrderId { get; set; }
    }
}