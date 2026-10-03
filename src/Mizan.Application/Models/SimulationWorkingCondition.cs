using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Simülatörün çalışma listesindeki bir deneme (S76-4, 5): kullanıcının kurduğu istek, açık/kapalı tercihi ve
/// okunduğu gün hesaba girmesini engelleyen bir sorun varsa o. Sorun saklanmaz, her okumada bugüne göre
/// değerlendirilir; dün geçerli olan deneme bugün tarihi geçmiş olabilir.
/// </summary>
/// <param name="Request">Denemenin kendisi: tür, ad, tutar, tarih.</param>
/// <param name="IsEnabled">Kullanıcı denemeyi açık mı bıraktı.</param>
/// <param name="Issue">Hesaba girmesini engelleyen sorun; yoksa <see cref="SimulationConditionIssue.None"/>.</param>
public sealed record SimulationWorkingCondition(
    SimulationRequest Request,
    bool IsEnabled,
    SimulationConditionIssue Issue);
