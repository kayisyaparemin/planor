using Mizan.Presentation.Dialogs;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde kullanıcıya gösterilen diyalog ve prompt etkileşimlerini simüle eden sahte servis.
/// </summary>
public sealed class FakeDialogService : IDialogService
{
    public string? LastAlertTitle { get; private set; }
    public string? LastAlertMessage { get; private set; }
    public string? NextPromptResponse { get; set; }
    public bool NextConfirmResponse { get; set; } = true;
    public string? NextChooseResponse { get; set; }

    /// <summary>Art arda seçimler için sıra; boşsa <see cref="NextChooseResponse"/> kullanılır.</summary>
    public Queue<string?> ChooseResponses { get; } = new();

    public string[]? LastChooseOptions { get; private set; }
    public string? LastChooseTitle { get; private set; }
    public string? LastChooseDestruction { get; private set; }
    public int ConfirmCount { get; private set; }
    public string? LastConfirmTitle { get; private set; }
    public string? LastConfirmMessage { get; private set; }

    public Task ShowAlertAsync(string title, string message, string button = "Tamam")
    {
        LastAlertTitle = title;
        LastAlertMessage = message;
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message, string accept = "Evet", string cancel = "Hayır")
    {
        ConfirmCount++;
        LastConfirmTitle = title;
        LastConfirmMessage = message;
        return Task.FromResult(NextConfirmResponse);
    }

    public Task<string?> PromptAsync(string title, string message, string accept = "Tamam", string cancel = "Vazgeç", string placeholder = "", int maxLength = -1)
    {
        return Task.FromResult(NextPromptResponse);
    }

    public Task<string?> ChooseAsync(string title, string cancel, string? destruction, params string[] options)
    {
        LastChooseOptions = options;
        LastChooseTitle = title;
        LastChooseDestruction = destruction;
        return Task.FromResult(ChooseResponses.Count > 0 ? ChooseResponses.Dequeue() : NextChooseResponse);
    }
}
