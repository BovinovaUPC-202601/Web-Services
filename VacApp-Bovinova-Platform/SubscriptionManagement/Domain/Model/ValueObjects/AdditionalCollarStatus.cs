namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

/// <summary>
/// Lifecycle of an additional collar request (TP TS023):
/// requested → approved → delivered (or cancelled). Approved/Delivered count
/// toward the user's collar allowance and the monthly bill.
/// </summary>
public enum AdditionalCollarStatus
{
    Requested,
    Approved,
    Delivered,
    Cancelled
}
