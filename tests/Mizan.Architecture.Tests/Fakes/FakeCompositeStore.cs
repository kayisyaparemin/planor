using Mizan.Application.Abstractions;

namespace Mizan.Architecture.Tests.Fakes;

/// <summary>
/// Kompozit arayüz mimari kuralının ihlalleri yakaladığını test etmek amacıyla birden fazla
/// depo portunu miras alan kasıtlı hatalı sahte depo arayüzü.
/// </summary>
internal interface IFakeCompositeStore : ILoanRepository, ICreditCardRepository;
