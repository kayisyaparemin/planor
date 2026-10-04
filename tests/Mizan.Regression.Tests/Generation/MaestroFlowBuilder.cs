using System.Globalization;
using System.Text;

namespace Mizan.Regression.Tests.Generation;

/// <summary>
/// Maestro E2E test komutlarını tip-güvenli olarak üreten akıcı sözdizimi oluşturucusu.
/// </summary>
public sealed class MaestroFlowBuilder
{
    private readonly StringBuilder _builder = new();
    private readonly List<string> _referencedIds = [];

    /// <summary>
    /// Akış boyunca referans verilen otomasyon kimliklerinin salt-okunur listesi.
    /// </summary>
    public IReadOnlyList<string> ReferencedIds => _referencedIds;

    /// <summary>
    /// Uygulamayı belirtilen paket kimliği ve durum temizliği seçeneğiyle başlatır.
    /// </summary>
    public MaestroFlowBuilder LaunchApp(string appId, bool clearState = true)
    {
        _builder.AppendLine(CultureInfo.InvariantCulture, $"appId: {appId}");
        _builder.AppendLine("---");
        _builder.AppendLine("- launchApp:");
        _builder.AppendLine(CultureInfo.InvariantCulture, $"    clearState: {clearState.ToString().ToLowerInvariant()}");
        return this;
    }

    /// <summary>
    /// Akışa görsel bir bölüm başlığı yorumu ekler.
    /// </summary>
    public MaestroFlowBuilder Section(string title)
    {
        _builder.AppendLine();
        _builder.AppendLine("# ============================================================");
        _builder.AppendLine(CultureInfo.InvariantCulture, $"# {title}");
        _builder.AppendLine("# ============================================================");
        return this;
    }

    /// <summary>
    /// Belirtilen AutomationId kimliğine sahip öğeye dokunma komutu ekler.
    /// </summary>
    public MaestroFlowBuilder TapOnId(string automationId)
    {
        _referencedIds.Add(automationId);
        _builder.AppendLine("- tapOn:");
        _builder.AppendLine(CultureInfo.InvariantCulture, $"    id: \"{automationId}\"");
        return this;
    }

    /// <summary>
    /// Belirtilen metne dokunma komutu ekler (erişilebilirlik veya sistem diyalogları için).
    /// </summary>
    public MaestroFlowBuilder TapOnText(string text)
    {
        _builder.AppendLine(CultureInfo.InvariantCulture, $"- tapOn: \"{text}\"");
        return this;
    }

    /// <summary>
    /// Aktif odaklı alana metin yazma komutu ekler.
    /// </summary>
    public MaestroFlowBuilder InputText(string text)
    {
        _builder.AppendLine(CultureInfo.InvariantCulture, $"- inputText: \"{text}\"");
        return this;
    }

    /// <summary>
    /// Belirtilen AutomationId kimliğine sahip öğenin görünür olduğunu doğrulayan komut ekler.
    /// </summary>
    public MaestroFlowBuilder AssertVisibleId(string automationId)
    {
        _referencedIds.Add(automationId);
        _builder.AppendLine("- assertVisible:");
        _builder.AppendLine(CultureInfo.InvariantCulture, $"    id: \"{automationId}\"");
        return this;
    }

    /// <summary>
    /// Geri tuşuna basma komutu ekler.
    /// </summary>
    public MaestroFlowBuilder Back()
    {
        _builder.AppendLine("- back");
        return this;
    }

    /// <summary>
    /// Üretilen Maestro YAML metnini döndürür.
    /// </summary>
    public string BuildYaml()
    {
        return _builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd() + "\n";
    }
}
