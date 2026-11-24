namespace Magazin.Domain.Workflows;

/// <summary>
/// Orașele în care livrează magazinul. Este configurație, nu date din baza de date (schema din Lucrarea 4 nu are un
/// tabel pentru ele), deci vine din <c>appsettings.json</c> și se transmite workflow-ului ca valoare.
/// </summary>
public sealed record DeliveryZone(IReadOnlySet<string> Cities);
