namespace Magazin.Domain.States;

/// <summary>Adresa de livrare exact cum a fost introdusă, înainte de validare.</summary>
public sealed record UnvalidatedAddress(string? Street, string? City, string? PostalCode);
