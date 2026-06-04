using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.AlertManagement.Application.Internal.QueryServices;

public class AlertQueryService(IAlertRepository alertRepository) : IAlertQueryService
{
    public async Task<IEnumerable<Alert>> Handle(GetAlertsByUserIdQuery query)
        => await alertRepository.FindByUserIdAsync(query.UserId);

    public async Task<IEnumerable<Alert>> Handle(GetAlertsByUserIdAndBovineIdQuery query)
        => await alertRepository.FindByUserIdAndBovineIdAsync(query.UserId, query.BovineId, query.Limit);

    public async Task<Alert?> Handle(GetAlertByIdQuery query)
        => await alertRepository.FindByIdAsync(query.AlertId);
}
