namespace Divergent.Finance.Messages
{
    public class ProcessPaymentMessage
    {
        public int CustomerId { get; set; }
        public double Amount { get; set; }
        public int OrderId { get; set; }
    }
}