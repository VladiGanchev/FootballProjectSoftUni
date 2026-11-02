using X.PagedList;

namespace FootballProjectSoftUni.Core.Models.AdminPayments
{
    /// <summary>One page of the admin "Payments" list, with the totals of all payments for the tiles above it.</summary>
    public class AdminPaymentsPage
    {
        public const int PageSize = 25;

        /// <summary>Filters of the status pills ("?status="); anything else means all payments.</summary>
        public const string Paid = "paid";
        public const string Refunded = "refunded";
        public const string Pending = "pending";
        public const string Other = "other";

        public IPagedList<AdminPaymentViewModel> Payments { get; set; } = null!;

        /// <summary>The chosen status filter, or null for all payments.</summary>
        public string? Status { get; set; }

        public string? Search { get; set; }

        // Totals of all payments, whatever the filter and search.
        public int AllCount { get; set; }
        public int PaidCount { get; set; }
        public int RefundedCount { get; set; }
        public int PendingCount { get; set; }
        public int OtherCount => AllCount - PaidCount - RefundedCount - PendingCount;
        public decimal PaidTotal { get; set; }
    }
}
