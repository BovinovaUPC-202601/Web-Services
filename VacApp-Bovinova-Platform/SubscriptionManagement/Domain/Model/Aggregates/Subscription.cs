using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;

/// <summary>
/// A user's subscription. Owns the plan, lifecycle status and billing dates (TP EP009).
/// One subscription per user; defaults to Free with no IA/IoT/collar access.
/// </summary>
public class Subscription : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] public DateTimeOffset? CreatedDate { get; set; }
    [Column("UpdatedAt")] public DateTimeOffset? UpdatedDate { get; set; }

    public int                Id          { get; private set; }
    public int                UserId      { get; private set; }
    public PlanType           Plan        { get; private set; }
    public SubscriptionStatus Status      { get; private set; }
    public DateTime?          StartDate   { get; private set; }
    public DateTime?          NextRenewal { get; private set; }
    public DateTime?          SuspendedAt { get; private set; }

    protected Subscription() { }

    /// <summary>Creates the implicit Free subscription a user gets on sign-up.</summary>
    public Subscription(int userId)
    {
        UserId = userId;
        Plan   = PlanType.Free;
        Status = SubscriptionStatus.Active;
    }

    public bool IsPlusActive => Plan == PlanType.Plus && Status == SubscriptionStatus.Active;

    /// <summary>Activates Plus (simulated payment): sets dates and a one-month renewal.</summary>
    public void ActivatePlus()
    {
        Plan        = PlanType.Plus;
        Status      = SubscriptionStatus.Active;
        StartDate   = DateTime.UtcNow;
        NextRenewal = DateTime.UtcNow.AddMonths(1);
        SuspendedAt = null;
    }

    /// <summary>Suspends a Plus subscription (e.g. non-payment); premium access is blocked.</summary>
    public void Suspend()
    {
        if (Plan != PlanType.Plus) return;
        Status      = SubscriptionStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status      = SubscriptionStatus.Cancelled;
        SuspendedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        if (Plan == PlanType.Plus) Status = SubscriptionStatus.Expired;
    }
}
