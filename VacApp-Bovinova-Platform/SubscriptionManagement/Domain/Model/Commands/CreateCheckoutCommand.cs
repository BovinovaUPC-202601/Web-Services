using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Commands;

/// <summary>Opens a hosted checkout for a paid concept (Plus base or additional collar).</summary>
public record CreateCheckoutCommand(int UserId, string UserEmail, PaymentConcept Concept);
