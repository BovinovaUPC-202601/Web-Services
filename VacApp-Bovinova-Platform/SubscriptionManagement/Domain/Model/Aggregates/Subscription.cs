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

    /// <summary>NextRenewal value the 10-day reminder was last sent for (null = never).
    /// Compared against the current NextRenewal so each billing cycle reminds once.</summary>
    public DateTime?          Reminder10SentForRenewal { get; private set; }

    /// <summary>NextRenewal value the 5-day reminder was last sent for.</summary>
    public DateTime?          Reminder5SentForRenewal  { get; private set; }

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
        // New cycle → reminders for the previous renewal no longer apply.
        Reminder10SentForRenewal = null;
        Reminder5SentForRenewal  = null;
    }

    /// <summary>
    /// True if the given reminder stage still needs to be sent for the current
    /// <see cref="NextRenewal"/> cycle (i.e. it hasn't already been stamped for it).
    /// </summary>
    public bool NeedsReminder(ReminderStage stage) => stage == ReminderStage.TenDays
        ? Reminder10SentForRenewal != NextRenewal
        : Reminder5SentForRenewal  != NextRenewal;

    /// <summary>Records that a reminder stage was sent for the current cycle, so it is not resent.</summary>
    public void MarkReminderSent(ReminderStage stage)
    {
        if (stage == ReminderStage.TenDays) Reminder10SentForRenewal = NextRenewal;
        else                                Reminder5SentForRenewal  = NextRenewal;
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
