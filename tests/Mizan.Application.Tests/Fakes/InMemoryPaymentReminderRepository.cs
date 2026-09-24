using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Tests.Fakes;

public sealed class InMemoryPaymentReminderRepository : IPaymentReminderRepository
{
    private PaymentReminderMode _mode = PaymentReminderMode.Off;
    private readonly Dictionary<string, PaymentReminderResponse> _responses = new();

    public Task<PaymentReminderMode> GetModeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_mode);

    public Task SaveModeAsync(PaymentReminderMode mode, CancellationToken cancellationToken = default)
    {
        _mode = mode;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PaymentReminderResponse>> GetResponsesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaymentReminderResponse> list = _responses.Values.ToList();
        return Task.FromResult(list);
    }

    public Task UpsertResponsesAsync(
        IReadOnlyList<PaymentReminderResponse> responses,
        CancellationToken cancellationToken = default)
    {
        foreach (var response in responses)
        {
            _responses[response.DueKey] = response;
        }

        return Task.CompletedTask;
    }

    public Task DeleteResponseAsync(string dueKey, CancellationToken cancellationToken = default)
    {
        _responses.Remove(dueKey);
        return Task.CompletedTask;
    }
}
