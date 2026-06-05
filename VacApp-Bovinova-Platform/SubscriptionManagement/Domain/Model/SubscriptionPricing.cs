namespace VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model;

/// <summary>
/// Pricing rules for the Plus plan (TP EP009 / TS018).
/// Base S/149/month includes 3 collars; each extra active collar costs S/25/month.
/// </summary>
public static class SubscriptionPricing
{
    public const decimal PlusBaseMonthly  = 149m;
    public const decimal AdditionalCollarMonthly = 25m;
    public const int     IncludedCollars  = 3;

    /// <summary>
    /// Monthly cost = base + 25 × (active collars beyond the 3 included).
    /// </summary>
    public static decimal MonthlyCost(int activeCollars)
    {
        var additional = Math.Max(0, activeCollars - IncludedCollars);
        return PlusBaseMonthly + AdditionalCollarMonthly * additional;
    }
}
