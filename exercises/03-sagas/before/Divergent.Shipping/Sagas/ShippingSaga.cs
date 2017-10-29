using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Divergent.Finance.Messages.Events;
using Divergent.Sales.Messages.Events;
using NServiceBus;

namespace Divergent.Shipping.Sagas
{
    public class ShippingSaga : Saga<ShippingSaga.ShippingSagaData>, IAmStartedByMessages<OrderSubmittedEvent>,
        IAmStartedByMessages<PaymentSucceededEvent>
    {
        public Task Handle(OrderSubmittedEvent message, IMessageHandlerContext context)
        {
            Data.OrderId = message.OrderId;
            Data.CustomerId = message.CustomerId;

            Data.IsOrderSubmitted = true;

            var projection = message.Products.Select(p => new ShippingSagaData.Product {Identifier = p});
            Data.Products = projection.ToList();
            return ProcessOrder(context);
        }

        public Task Handle(PaymentSucceededEvent message, IMessageHandlerContext context)
        {
            Data.OrderId = message.OrderId;
            Data.IsPaymentProceeded = true;
            return ProcessOrder(context);
        }

        public async Task ProcessOrder(IMessageHandlerContext context)
        {
            if (Data.IsOrderSubmitted && Data.IsPaymentProceeded)
            {
                await Task.CompletedTask;
                MarkAsComplete();
            }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingSagaData> mapper)
        {
            mapper.ConfigureMapping<OrderSubmittedEvent>(p => p.OrderId).ToSaga(s => s.OrderId);
            mapper.ConfigureMapping<PaymentSucceededEvent>(p => p.OrderId).ToSaga(s => s.OrderId);
        }

        public class ShippingSagaData : ContainSagaData
        {
            public virtual int OrderId { get; set; }

            public virtual int CustomerId { get; set; }

            public virtual ICollection<Product> Products { get; set; }
            public virtual bool IsPaymentProceeded { get; set; }
            public virtual bool IsOrderSubmitted { get; set; }

            public class Product
            {
                public virtual int Identifier { get; set; }
            }
        }
    }
}