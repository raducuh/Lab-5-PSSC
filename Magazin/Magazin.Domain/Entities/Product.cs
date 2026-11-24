using Magazin.Domain.ValueObjects;

namespace Magazin.Domain.Entities;

/// <summary>
/// Un produs din catalog. Tip entitate: identitatea este <see cref="Code"/>, iar două instanțe cu același cod
/// sunt același produs, chiar dacă numele sau prețul s-au schimbat între timp.
/// </summary>
public sealed class Product(ProductCode code, string name, Money unitPrice) : IEquatable<Product>
{
    public ProductCode Code { get; } = code;

    public string Name { get; } = name;

    public Money UnitPrice { get; } = unitPrice;

    public bool Equals(Product? other) => other is not null && Code == other.Code;

    public override bool Equals(object? obj) => Equals(obj as Product);

    public override int GetHashCode() => Code.GetHashCode();

    public override string ToString() => $"{Name} ({Code})";
}
