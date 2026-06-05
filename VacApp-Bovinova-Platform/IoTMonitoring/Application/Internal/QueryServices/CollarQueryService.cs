using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.QueryServices;

public class CollarQueryService(ICollarRepository collarRepository)
    : ICollarQueryService
{
    public async Task<IEnumerable<Collar>> Handle(GetCollarsByUserIdQuery query)
        => await collarRepository.FindByUserIdAsync(query.UserId);

    public async Task<int> GetActiveCountAsync(int userId)
        => await collarRepository.CountActiveByUserIdAsync(userId);
}
