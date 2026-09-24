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
        typeof(IProfileRepository)
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
}
