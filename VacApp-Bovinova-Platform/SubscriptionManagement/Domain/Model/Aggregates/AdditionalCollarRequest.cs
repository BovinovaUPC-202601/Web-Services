using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;

/// <summary>
/// A request for an extra collar slot beyond the 3 included in Plus (TP US029/TS023).
/// Each approved/delivered request raises the user's collar allowance by one and adds
/// its monthly amount (S/25) to the subscription bill while the collar stays active.
/// </summary>
public class AdditionalCollarRequest : IEntityWithCreatedUpdatedDate
{
    [Column("CreatedAt")] public DateTimeOffset? CreatedDate { get; set; }
    [Column("UpdatedAt")] public DateTimeOffset? UpdatedDate { get; set; }

    public int                    Id            { get; private set; }
    public int                    UserId        { get; private set; }
    public AdditionalCollarStatus Status        { get; private set; }
    public decimal                MonthlyAmount { get; private set; }
    public DateTime               RequestedAt   { get; private set; }

    protected AdditionalCollarRequest() { }

    public AdditionalCollarRequest(int userId, decimal monthlyAmount)
    {
        UserId        = userId;
        MonthlyAmount = monthlyAmount;
        RequestedAt   = DateTime.UtcNow;
        Status        = AdditionalCollarStatus.Requested;
    }

    public void Approve()  => Status = AdditionalCollarStatus.Approved;
    public void Deliver()  => Status = AdditionalCollarStatus.Delivered;
    public void Cancel()   => Status = AdditionalCollarStatus.Cancelled;

    /// <summary>Counts toward allowance and billing.</summary>
    public bool IsActive => Status is AdditionalCollarStatus.Approved or AdditionalCollarStatus.Delivered;
}
