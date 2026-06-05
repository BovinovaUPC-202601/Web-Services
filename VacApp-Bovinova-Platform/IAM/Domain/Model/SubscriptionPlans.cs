namespace VacApp_Bovinova_Platform.IAM.Domain.Model
{
    /// <summary>
    /// Canonical subscription plan names. Avoids magic strings scattered across
    /// controllers and authorization filters.
    /// </summary>
    public static class SubscriptionPlans
    {
        public const string Free = "Free";
        public const string Plus = "Plus";

        public static bool IsValid(string plan) => plan is Free or Plus;
    }
}
