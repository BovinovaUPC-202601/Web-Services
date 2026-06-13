using VacApp_Bovinova_Platform.IAM.Application.OutBoundServices;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Commands;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.StaffAdministration.Application.Internal.CommandServices;

public class StaffCommandService(IStaffRepository staffRepository,
    IUserRepository userRepository,
    IHashingService hashingService,
    IUnitOfWork unitOfWork) : IStaffCommandService
{
    public async Task<Staff?> Handle(CreateStaffCommand command)
    {
        // Duplicate names are only a problem within the same ranch: two different
        // owners can perfectly have staff members with the same name.
        var ownerStaff = await staffRepository.FindByOwnerUserIdAsync(command.UserId);
        if (ownerStaff.Any(s => s.Name == command.Name))
            throw new ConflictException($"Staff entity with name '{command.Name}' already exists.");

        var staff = new Staff(command);

        await staffRepository.AddAsync(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<Staff?> Handle(UpdateStaffCommand command)
    {
        var staff = await staffRepository.FindByIdAsync(command.Id);
        if (staff == null)
            throw new NotFoundException($"Staff with ID '{command.Id}' not found.");

        staff.Update(command);

        staffRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<Staff?> Handle(DeleteStaffCommand command)
    {
        var staff = await staffRepository.FindByIdAsync(command.Id);
        if (staff == null)
            throw new NotFoundException($"Staff with ID '{command.Id}' not found.");

        // Only the staff record is removed; the linked User account (if any) is preserved.
        staffRepository.Remove(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<Staff?> Handle(CreateStaffWithNewUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ValidationException("Email is required.");
        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ValidationException("Password is required.");
        Staff.ValidateAccessLevel(command.AccessLevel);
        Staff.ValidateEmail(command.Email);

        var existingUser = await userRepository.FindByEmailAsync(command.Email);
        if (existingUser != null)
            throw new ConflictException($"A user with email '{command.Email}' already exists.");

        var duplicateStaff = await staffRepository.FindByOwnerUserIdAndEmailAsync(command.OwnerUserId, command.Email);
        if (duplicateStaff != null)
            throw new ConflictException("This ranch already has a staff member with that email.");

        // Create the real User account reusing the IAM hashing mechanism;
        // the plain password is never persisted.
        var user = new User(new SignUpCommand(
            command.Name, command.Email, hashingService.GenerateHash(command.Password)));
        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();

        var staff = new Staff(command.Name, command.Email, command.AccessLevel, command.OwnerUserId, user.Id);
        await staffRepository.AddAsync(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<Staff?> Handle(GrantStaffAccessToExistingUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ValidationException("Email is required.");
        Staff.ValidateAccessLevel(command.AccessLevel);

        var user = await userRepository.FindByEmailAsync(command.Email);
        if (user is null)
            throw new NotFoundException($"No user found with email '{command.Email}'.");

        if (user.Id == command.OwnerUserId)
            throw new ValidationException("You cannot add yourself as staff of your own ranch.");

        if (await staffRepository.FindByOwnerUserIdAndLinkedUserIdAsync(command.OwnerUserId, user.Id) != null)
            throw new ConflictException("That user already has staff access on this ranch.");

        if (await staffRepository.FindByOwnerUserIdAndEmailAsync(command.OwnerUserId, user.Email) != null)
            throw new ConflictException("This ranch already has a staff member with that email.");

        var name = string.IsNullOrWhiteSpace(user.Username) ? user.Email : user.Username;
        var staff = new Staff(name, user.Email, command.AccessLevel, command.OwnerUserId, user.Id);
        await staffRepository.AddAsync(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }

    public async Task<Staff?> Handle(UpdateStaffAccessCommand command)
    {
        var staff = await staffRepository.FindByIdAsync(command.Id);
        // NotFound (never Forbidden) when the record belongs to another owner,
        // so the endpoint does not leak the existence of foreign staff ids.
        if (staff is null || staff.UserId != command.OwnerUserId)
            throw new NotFoundException("Staff not found.");

        if (command.EmployeeStatus is not (1 or 2))
            throw new ValidationException("EmployeeStatus must be 1 (Active) or 2 (Inactive) for access management.");
        Staff.ValidateAccessLevel(command.AccessLevel);

        staff.UpdateAccess(command.EmployeeStatus, command.AccessLevel);

        staffRepository.Update(staff);
        await unitOfWork.CompleteAsync();

        return staff;
    }
}
