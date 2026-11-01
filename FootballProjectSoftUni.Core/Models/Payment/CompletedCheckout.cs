namespace FootballProjectSoftUni.Core.Models.Payment
{
    /// <summary>
    /// The parts of a completed Stripe Checkout session that are needed to confirm a payment.
    /// </summary>
    public class CompletedCheckout
    {
        public string SessionId { get; set; } = null!;

        /// <summary>orderId from the session metadata (only checked against the order found by SessionId).</summary>
        public string? OrderId { get; set; }

        public string? PaymentStatus { get; set; }

        /// <summary>Amount charged, in cents.</summary>
        public long? AmountTotal { get; set; }

        public string? Currency { get; set; }

        public string? PaymentIntentId { get; set; }
    }

    public enum CheckoutPaymentOutcome
    {
        /// <summary>The order was waiting for this payment and is now Paid: register the team.</summary>
        Join,

        /// <summary>Money was taken for an order that is no longer active (e.g. Expired): refund it.</summary>
        Refund,

        /// <summary>The order was already handled (paid, refunded...) - e.g. Stripe sent the event again.</summary>
        AlreadyProcessed,

        /// <summary>No order on this site has this session (e.g. a payment made on another copy of the site).</summary>
        UnknownSession,

        /// <summary>The session does not match its order (amount, currency, order id) or is not paid.</summary>
        Rejected
    }

    public class CheckoutPaymentResult
    {
        public CheckoutPaymentResult(CheckoutPaymentOutcome outcome, FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment? order = null, string? reason = null)
        {
            Outcome = outcome;
            Order = order;
            Reason = reason;
        }

        public CheckoutPaymentOutcome Outcome { get; }

        public FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment? Order { get; }

        public string? Reason { get; }
    }
}
