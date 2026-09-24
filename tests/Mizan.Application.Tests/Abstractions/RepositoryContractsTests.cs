using System.Reflection;
using Mizan.Application.Abstractions;

namespace Mizan.Application.Tests.Abstractions;

public sealed class RepositoryContractsTests
{
    private static readonly Type[] RepositoryTypes =
    [
        typeof(ILoanRepository),
        typeof(ICreditCardRepository),
        typeof(ITemporaryPaymentPlanRepository),
        typeof(IPlannedLargeExpenseRepository),
        typeof(IRecurringIncomeRepository),
        typeof(IAdHocIncomeRepository),
        typeof(IUserSettingsRepository),
        typeof(IProfileRepository),
        typeof(IPeriodHistoryRepository),
        typeof(IPeriodObservationRepository),
        typeof(IPaymentReminderRepository),
        typeof(ISimulationDraftRepository)
    ];

    [Fact]
    public void TumDepoPortlari_ArayuzOlmalidir()
    {
        foreach (var type in RepositoryTypes)
        {
            Assert.True(type.IsInterface, $"{type.Name} bir interface olmalıdır.");
        }
    }

    [Fact]
    public void TumDepoPortlari_KuralM5_OnMetottanAzOlmali()
    {
        // Kural M5: Tanrı arayüz yasak; bir arayüzde 10'dan fazla metot olamaz.
        foreach (var type in RepositoryTypes)
        {
            var methodCount = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Length;
            Assert.True(methodCount <= 10, $"{type.Name} M5 kuralını ihlal ediyor: {methodCount} metot var (en fazla 10 olabilir).");
        }
    }

    [Fact]
    public void TumDepoMetotlari_AsenkronVeCancellationTokenTasiyicisiOlmali()
    {
        foreach (var type in RepositoryTypes)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                Assert.True(
                    typeof(Task).IsAssignableFrom(method.ReturnType),
                    $"{type.Name}.{method.Name} metodu Task döndürmelidir.");

                var parameters = method.GetParameters();
                Assert.NotEmpty(parameters);

                var lastParam = parameters[^1];
                Assert.Equal(typeof(CancellationToken), lastParam.ParameterType);
                Assert.True(lastParam.HasDefaultValue, $"{type.Name}.{method.Name} son parametresi varsayılan CancellationToken taşımalıdır.");
            }
        }
    }

    [Fact]
    public void TumDepoPortlari_BagimsizOlmalidir_KompozitDepoYasak()
    {
        // Kural M5 ve Düğüm T10: Hiçbir depo portu başka bir depoyu miras alamaz; kompozit tanrı arayüzler yasaktır.
        foreach (var type in RepositoryTypes)
        {
            var inheritedRepoInterfaces = type.GetInterfaces()
                .Where(i => RepositoryTypes.Contains(i))
                .ToList();

            Assert.Empty(inheritedRepoInterfaces);
        }
    }

    [Fact]
    public void TumDepoPortlari_EksiksizListelenmisOlmali()
    {
        var applicationAssembly = typeof(ILoanRepository).Assembly;
        var allRepoInterfaces = applicationAssembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.EndsWith("Repository", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(RepositoryTypes.Length, allRepoInterfaces.Count);
        foreach (var repo in allRepoInterfaces)
        {
            Assert.Contains(repo, RepositoryTypes);
        }
    }

    [Fact]
    public void IMizanStore_TanriArayuzu_UretimdeVarOlamaz()
    {
        var applicationAssembly = typeof(ILoanRepository).Assembly;
        var mizanStoreType = applicationAssembly.GetType("Mizan.Application.Abstractions.IMizanStore");
        Assert.Null(mizanStoreType);
    }
}
