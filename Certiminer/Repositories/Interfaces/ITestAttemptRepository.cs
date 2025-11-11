using Certiminer.Models;

namespace Certiminer.Repositories.Interfaces;

public interface ITestAttemptRepository
{
    Task<TestAttempt?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<TestAttempt>> ListByUserAsync(string userId, CancellationToken ct = default); // requiere TestAttempt.UserId
    Task AddAsync(TestAttempt entity, CancellationToken ct = default);
    Task UpdateAsync(TestAttempt entity);
}
