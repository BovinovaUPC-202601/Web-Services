using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.AlertManagement.Application.Internal.CommandServices;

public class AlertCommandService(
    IAlertRepository alertRepository,
    IUnitOfWork unitOfWork)
    : IAlertCommandService
{
    public async Task<Alert?> Handle(RegisterAlertCommand command)
    {
        var alert = new Alert(command);
        try
        {
            await alertRepository.AddAsync(alert);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }
        return alert;
    }

    public async Task<Alert?> Handle(MarkAlertAsReadCommand command)
    {
        var alert = await alertRepository.FindByIdAsync(command.AlertId);
        if (alert is null) return null;

        alert.MarkAsRead();
        try
        {
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }
        return alert;
    }
}
