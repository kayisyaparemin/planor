using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Dönem kapanışı taahhüdünün (<see cref="PeriodSettlementCommit"/>) referans bütünlüğü,
/// takvim sürekliliği ve bakiye devri kurallarına uygunluğunu denetleyen saf doğrulayıcı.
/// Hatalı veya tutarsız bir kapanış paketinin veritabanına veya tarihçeye kaydedilmesini engellemek için vardır.
/// </summary>
public static class PeriodSettlementCommitValidator
{
    /// <summary>
    /// Verilen dönem kapanış taahhüdünü iş kurallarına göre denetler.
    /// Kural ihlali yoksa boş liste döndürür.
    /// </summary>
    /// <param name="commit">Doğrulanacak taahhüt paketi.</param>
    /// <returns>Tespit edilen kural ihlallerinin açıklayıcı hata mesajları listesi.</returns>
    public static IReadOnlyList<string> Validate(PeriodSettlementCommit commit)
    {
        if (commit is null)
        {
            return ["Taahhüt paketi boş olamaz."];
        }

        var errors = new List<string>();

        ValidateRequiredFields(commit, errors);
        if (errors.Count > 0)
        {
            return errors;
        }

        ValidateTimelineAndBalances(commit, errors);
        ValidateSnapshotReferences(commit, errors);
        ValidateDetails(commit, errors);

        return errors;
    }

    private static void ValidateRequiredFields(PeriodSettlementCommit commit, List<string> errors)
    {
        if (commit.Actual is null)
        {
            errors.Add("Kapanan dönemin fiilî gerçekleşme kaydı (Actual) zorunludur.");
        }

        if (commit.NewSnapshot is null)
        {
            errors.Add("Yeni dönemin finansal durum kaydı (NewSnapshot) zorunludur.");
        }

        if (commit.NewPlan is null)
        {
            errors.Add("Yeni dönemin dondurulmuş plan taahhüdü (NewPlan) zorunludur.");
        }

        if (commit.UpdatedSettings is null)
        {
            errors.Add("Kullanıcı ayarları (UpdatedSettings) zorunludur.");
        }
    }

    private static void ValidateTimelineAndBalances(PeriodSettlementCommit commit, List<string> errors)
    {
        if (commit.Actual.PeriodEnd != commit.NewPlan.PeriodStart)
        {
            errors.Add($"Kapanan dönemin bitiş tarihi ({commit.Actual.PeriodEnd:yyyy-MM-dd}), yeni dönemin başlangıç tarihi ({commit.NewPlan.PeriodStart:yyyy-MM-dd}) ile eşleşmelidir.");
        }

        if (commit.Actual.ConfirmedEndingBalance != commit.NewSnapshot.ProjectionOpeningBalance)
        {
            errors.Add($"Yeni finansal durumun açılış bakiyesi ({commit.NewSnapshot.ProjectionOpeningBalance:N2}), teyit edilen kapanış bakiyesi ({commit.Actual.ConfirmedEndingBalance:N2}) ile eşleşmelidir.");
        }

        if (commit.Actual.ConfirmedEndingBalance != commit.NewPlan.OpeningBalance)
        {
            errors.Add($"Yeni dönemin açılış bakiyesi ({commit.NewPlan.OpeningBalance:N2}), teyit edilen kapanış bakiyesi ({commit.Actual.ConfirmedEndingBalance:N2}) ile eşleşmelidir.");
        }
    }

    private static void ValidateSnapshotReferences(PeriodSettlementCommit commit, List<string> errors)
    {
        if (commit.NewPlan.FinancialSnapshotId != commit.NewSnapshot.Id)
        {
            errors.Add("Yeni dönemin planı (NewPlan), yeni finansal duruma (NewSnapshot) bağlı olmalıdır.");
        }

        if (commit.Actual.ResultFinancialSnapshotId != commit.NewSnapshot.Id)
        {
            errors.Add("Kapanan dönemin sonuç finansal durum kimliği (ResultFinancialSnapshotId), yeni finansal duruma (NewSnapshot) işaret etmelidir.");
        }
    }

    private static void ValidateDetails(PeriodSettlementCommit commit, List<string> errors)
    {
        if (commit.Revision is not null && commit.Revision.PeriodPlanSnapshotId != commit.Actual.PeriodPlanSnapshotId)
        {
            errors.Add("Plan revizyonu, kapanan dönemin dondurulmuş planına ait olmalıdır.");
        }

        if (commit.RemovedLoanPrepaymentIds.Any(id => id == Guid.Empty))
        {
            errors.Add("Tüketilen erken ödeme kimlikleri arasında geçersiz (boş) GUID bulunamaz.");
        }
    }
}
