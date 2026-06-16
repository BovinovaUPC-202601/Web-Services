using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;

/// <summary>
/// A payment processed through the gateway. Records the money flow behind a
/// subscription change, keeping the <see cref="Subscription"/> aggregate free of
/// gateway concerns. A subscription is only activated when its payment transitions
/// to <see cref="PaymentStatus.Paid"/> via confirmation. One row per checkout attempt.
/// </summary>
public class Payment : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] public DateTimeOffset? CreatedDate { get; set; }
    [Column("UpdatedAt")] public DateTimeOffset? UpdatedDate { get; set; }

    public int            Id             { get; private set; }
    public int            UserId         { get; private set; }
    public PaymentConcept Concept        { get; private set; }
    public decimal        Amount         { get; private set; }
    public string         Currency       { get; private set; } = "PEN";
    public PaymentStatus  Status         { get; private set; }

    /// <summary>Gateway reference (checkout session id) once known.</summary>
    public string?        ProviderRef    { get; private set; }

    /// <summary>Caller-supplied key to avoid creating duplicate payments for one checkout.</summary>
    public string         IdempotencyKey { get; private set; } = string.Empty;

    public DateTime?      PaidAt         { get; private set; }

    protected Payment() { }

    public Payment(int userId, PaymentConcept concept, decimal amount, string idempotencyKey)
    {
        UserId         = userId;
        Concept        = concept;
        Amount         = amount;
        Currency       = "PEN";
        Status         = PaymentStatus.Pending;
        IdempotencyKey = idempotencyKey;
    }

    /// <summary>Stores the gateway reference when the checkout session is created (still pending).</summary>
    public void LinkProviderRef(string providerRef) => ProviderRef = providerRef;

    /// <summary>
    /// Confirms the payment. Idempotent: a repeated confirmation leaves state
    /// untouched, so the first confirmation wins.
    /// </summary>
    public void MarkPaid(string providerRef)
    {
        if (Status != PaymentStatus.Pending) return;
        Status      = PaymentStatus.Paid;
        ProviderRef = providerRef;
        PaidAt      = DateTime.UtcNow;
    }

    /// <summary>Marks the payment as failed. No-op once paid/refunded.</summary>
    public void MarkFailed()
    {
        if (Status != PaymentStatus.Pending) return;
        Status = PaymentStatus.Failed;
    }

    /// <summary>Refunds a paid payment. No-op unless currently paid.</summary>
    public void Refund()
    {
        if (Status != PaymentStatus.Paid) return;
        Status = PaymentStatus.Refunded;
    }
}
