using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Simülatörün sonuç kartı testlerinde hesabı taklit eden test çiftidir: verilen listeleri kaydeder, istenirse hata
/// fırlatır ya da cevabı testin elindeki bir görev tamamlanana kadar bekletir (üst üste binen istekler için).
/// </summary>
public sealed class FakeSimulationResultService : ISimulationResultService
{
    private readonly Queue<TaskCompletionSource<SimulationOutcome?>> _pending = new();

    /// <summary>Cevap dönecek sonuç; <c>null</c> açık dönem yok ya da zincir kurulamıyor demektir.</summary>
    public SimulationOutcome? Outcome { get; set; }

    /// <summary>Doluysa hesap bu hatayı fırlatır.</summary>
    public Exception? Failure { get; set; }

    /// <summary>Hesap istenen her listenin kaydı, sırasıyla.</summary>
    public List<IReadOnlyList<SimulationDraftCondition>> Calls { get; } = [];

    /// <summary>Sıradaki çağrının cevabını testin tamamlayacağı göreve bağlar; kuyruk boşalınca <see cref="Outcome"/>.</summary>
    public TaskCompletionSource<SimulationOutcome?> HoldNext()
    {
        var gate = new TaskCompletionSource<SimulationOutcome?>();
        _pending.Enqueue(gate);
        return gate;
    }

    /// <inheritdoc />
    public Task<SimulationOutcome?> CalculateAsync(
        IReadOnlyList<SimulationDraftCondition> conditions,
        CancellationToken cancellationToken = default)
    {
        Calls.Add(conditions);
        if (Failure is not null)
        {
            return Task.FromException<SimulationOutcome?>(Failure);
        }

        return _pending.Count > 0 ? _pending.Dequeue().Task : Task.FromResult(Outcome);
    }
}
