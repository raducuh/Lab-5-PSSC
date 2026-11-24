using Magazin.Domain.Errors;
using Magazin.Domain.States;
using Magazin.Domain.ValueObjects;
using Magazin.Functional;

namespace Magazin.Domain.Operations;

/// <summary>
/// Validarea comenzii: transformă datele brute (<see cref="Order.Unvalidated"/>) în obiecte-valoare
/// (<see cref="Order.Validated"/>), sau întoarce <b>toate</b> erorile găsite (nu doar prima) — validare „aplicativă”.
/// </summary>
public static class OrderValidation
{
    extension(Order.Unvalidated order)
    {
        /// <summary>Operație pură: nu citește nimic din afară. Verifică doar forma datelor, nu existența clientului, a produselor sau stocul.</summary>
        public Result<Order.Validated, IReadOnlyList<ValidationError>> Validate() =>
            Result.Combine(
                    ValidateCustomerCode(order.CustomerCode),
                    ValidateLines(order.Lines),
                    ValidateAddress(order.DeliveryAddress),
                    (customerCode, lines, address) => new Order.Validated(customerCode, lines, address))
                .MapError(errors => (IReadOnlyList<ValidationError>)[.. errors.SelectMany(group => group)]);
    }

    private static Result<CustomerCode, IReadOnlyList<ValidationError>> ValidateCustomerCode(string? raw) =>
        CustomerCode.Create(raw)
            .MapError(error => (IReadOnlyList<ValidationError>)[new ValidationError.InvalidCustomerCode(error.Raw)]);

    private static Result<IReadOnlyList<ValidatedOrderLine>, IReadOnlyList<ValidationError>> ValidateLines(IReadOnlyList<UnvalidatedOrderLine> lines)
    {
        if (lines.Count == 0)
        {
            return new List<ValidationError> { new ValidationError.EmptyOrder() };
        }

        return lines.Traverse(ValidateLine);
    }

    // Combină cele două conversii independente ale unei linii și adună toate erorile ei (nu se oprește la prima).
    private static Result<ValidatedOrderLine, IReadOnlyList<ValidationError>> ValidateLine(UnvalidatedOrderLine line) =>
        Result.Combine(
            ProductCode.Create(line.ProductCode)
                .MapError(error => (ValidationError)new ValidationError.InvalidProductCode(error.Raw)),
            Quantity.Parse(line.Quantity)
                .MapError(error => (ValidationError)new ValidationError.InvalidQuantity(line.ProductCode, error)),
            (productCode, quantity) => new ValidatedOrderLine(productCode, quantity));

    private static Result<Address, IReadOnlyList<ValidationError>> ValidateAddress(UnvalidatedAddress address) =>
        Address.Create(address.Street, address.City, address.PostalCode)
            .MapError(errors => (IReadOnlyList<ValidationError>)[.. errors.Select(error => new ValidationError.InvalidDeliveryAddress(error))]);
}
