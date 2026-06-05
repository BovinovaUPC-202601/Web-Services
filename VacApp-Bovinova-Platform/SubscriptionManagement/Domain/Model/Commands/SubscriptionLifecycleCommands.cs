namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;

/// <summary>Suspends a user's Plus subscription (simulated non-payment) (TP TS019).</summary>
public record SuspendSubscriptionCommand(int UserId);

/// <summary>Cancels a user's subscription.</summary>
public record CancelSubscriptionCommand(int UserId);

/// <summary>Admin approves a pending additional-collar request (TP TS023).</summary>
public record ApproveAdditionalCollarCommand(int RequestId);

/// <summary>Admin marks an approved additional collar as delivered/activated (TP TS023).</summary>
public record DeliverAdditionalCollarCommand(int RequestId);
