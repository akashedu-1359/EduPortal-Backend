namespace EduPortal.Application.Interfaces;

public interface IRevalidationService
{
    Task TriggerRevalidationAsync(string tag, CancellationToken ct = default);
}
