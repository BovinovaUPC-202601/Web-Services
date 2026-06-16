using MediatR;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Events;

/// <summary>
/// Published when a user's Plus plan ends (subscription suspended). AlertManagement
/// subscribes to raise a collar-return alert. <see cref="CollarCount"/> is how many IoT
/// collars the user holds and must return.
/// </summary>
public record SubscriptionEndedEvent(int UserId, int CollarCount) : INotification;
