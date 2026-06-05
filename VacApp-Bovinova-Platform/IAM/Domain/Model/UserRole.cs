namespace VacApp_Bovinova_Platform.IAM.Domain.Model
{
    /// <summary>
    /// Application roles. Ranchers manage their own ganado; Admins manage collar
    /// lifecycle, approve additional-collar requests and run recovery reports (TP).
    /// </summary>
    public enum UserRole
    {
        Rancher,
        Admin
    }
}
