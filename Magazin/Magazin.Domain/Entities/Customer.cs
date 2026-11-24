using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Entities;

/// <summary>
/// Clientul magazinului. Tip entitate: identitatea este <see cref="Code"/>, iar două instanțe cu același cod sunt
/// același client, chiar dacă numele s-a schimbat între timp. Clientul nu se creează în domeniu: se citește din baza
/// de date prin <see cref="Repositories.ICustomersRepository"/>, iar workflow-ul verifică doar că există.
/// </summary>
public sealed class Customer(CustomerCode code, string name) : IEquatable<Customer>
{
    public CustomerCode Code { get; } = code;

    public string Name { get; } = name;

    public bool Equals(Customer? other) => other is not null && Code == other.Code;

    public override bool Equals(object? obj) => Equals(obj as Customer);

    public override int GetHashCode() => Code.GetHashCode();

    public override string ToString() => $"{Name} ({Code})";
}
