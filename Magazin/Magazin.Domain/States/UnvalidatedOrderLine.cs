namespace Magazin.Domain.States;

/// <summary>O linie de comandă exact cum a fost introdusă (text brut, posibil lipsă sau greșit), înainte de orice validare.</summary>
public sealed record UnvalidatedOrderLine(string? ProductCode, string? Quantity);
