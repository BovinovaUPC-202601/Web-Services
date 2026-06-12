using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Queries;

namespace VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

public interface IStaffQueryService
{
    Task<IEnumerable<Staff>> Handle(GetAllStaffQuery query);
    Task<Staff?> Handle(GetStaffByIdQuery query);
    Task<IEnumerable<Staff>> Handle(GetStaffByEmployeeStatusQuery query);
    Task<int> CountStaffsByUserIdAsync(int userId);
}