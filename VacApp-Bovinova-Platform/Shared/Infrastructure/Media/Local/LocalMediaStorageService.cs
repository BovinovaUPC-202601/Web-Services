using VacApp_Bovinova_Platform.Shared.Application.OutboundServices;

namespace VacApp_Bovinova_Platform.Shared.Infrastructure.Media.Local;

public class LocalMediaStorageService : IMediaStorageService
{
    public string UploadFileAsync(string fileName, Stream fileData)
    {
        var safeFileName = string.Join(
            "-",
            fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(safeFileName))
            safeFileName = "bovine";

        return $"local://bovines/{safeFileName.ToLowerInvariant()}-{Guid.NewGuid():N}.webp";
    }

    public void UpdateFileAsync(string url, Stream fileData)
    {
        // Local development storage only returns stable fake URLs; no file replacement is needed.
    }
}
