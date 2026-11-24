using Magazin.Data.Queries;
using Magazin.Domain.Commands;
using Magazin.Domain.Errors;
using Magazin.Domain.Events;
using Magazin.Domain.States;
using Magazin.Domain.Workflows;
using Magazin.Functional;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Magazin.ConsoleApp;

/// <summary>
/// Meniul în linie de comandă. Fiecare acțiune rulează într-un <c>AsyncServiceScope</c> propriu (deci cu un
/// <c>ShoppingContext</c> propriu): o acțiune eșuată nu lasă în context modificări nesalvate care să strice acțiunile următoare.
/// </summary>
internal static class ShopMenu
{
    public static async Task RunAsync(IServiceProvider services)
    {
        while (true)
        {
            PrintMenu();
            string? choice = Console.ReadLine()?.Trim();
            if (choice is null)
            {
                return;
            }

            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await PlaceOrderAsync(services);
                        break;
                    case "2":
                        await ShowProductsAsync(services);
                        break;
                    case "3":
                        await ShowCustomersAsync(services);
                        break;
                    case "4":
                        await ShowOrdersAsync(services);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opțiune necunoscută.");
                        break;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Comanda nu a fost salvată: stocul s-a modificat în timp ce o plasați. Încercați din nou.");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine($"Comanda nu a fost salvată: {exception.InnerException?.Message ?? exception.Message}");
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine($"Comanda nu a fost salvată: {exception.Message}");
            }

            Console.WriteLine();
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine("=== Magazin virtual ===");
        Console.WriteLine("1. Plasează o comandă");
        Console.WriteLine("2. Afișează produsele (cu stoc)");
        Console.WriteLine("3. Afișează clienții");
        Console.WriteLine("4. Afișează comenzile");
        Console.WriteLine("0. Ieșire");
        Console.Write("Alegere: ");
    }

    // ---------- workflow ----------

    private static async Task PlaceOrderAsync(IServiceProvider services)
    {
        // Datele se trimit brute (text) workflow-ului: conversia în tipurile domeniului o face validarea.
        string? customerCode = Ask("Cod client (ex. CL0001): ");

        List<UnvalidatedOrderLine> lines = [];
        Console.WriteLine("Introduceți produsele comandate. Linie goală la cod produs pentru a termina.");
        while (true)
        {
            string? productCode = Ask("Cod produs (ex. PR0001): ");
            if (string.IsNullOrWhiteSpace(productCode))
            {
                break;
            }

            lines.Add(new UnvalidatedOrderLine(productCode, Ask("Cantitate: ")));
        }

        UnvalidatedAddress address = new(Ask("Stradă: "), Ask("Oraș: "), Ask("Cod poștal (6 cifre): "));
        PlaceOrderCommand command = new(customerCode, lines, address);

        await using AsyncServiceScope scope = services.CreateAsyncScope();
        PlaceOrderWorkflow workflow = scope.ServiceProvider.GetRequiredService<PlaceOrderWorkflow>();
        Result<OrderPlacedEvent, PlaceOrderError> result = await workflow.ExecuteAsync(command, CancellationToken.None);

        Console.WriteLine();
        Console.WriteLine(result.Match(
            placed => Describe(placed),
            error => $"Comandă respinsă:{Environment.NewLine}{string.Join(Environment.NewLine, error.ToMessages().Select(message => "  - " + message))}"));
    }

    private static string Describe(OrderPlacedEvent placed) => string.Join(
        Environment.NewLine,
        [
            $"Comandă plasată și salvată (Id = {placed.OrderId})",
            $"Client: {placed.Customer}",
            .. placed.Lines.Select(line => $"  - {line.Product.Name} ({line.Product.Code}): {line.Quantity} × {line.Product.UnitPrice} = {line.LineTotal}"),
            $"Total: {placed.Total}",
            $"Livrare: {placed.DeliveryAddress}",
            $"Plasată la: {placed.PlacedAt:u}",
        ]);

    private static string? Ask(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    // ---------- afișare (model de citire, fără trecere prin domeniu) ----------

    private static async Task ShowProductsAsync(IServiceProvider services)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        IReadOnlyList<ProductRow> products = await scope.ServiceProvider.GetRequiredService<IShopQuery>().GetProductsAsync(CancellationToken.None);

        ConsoleTable.Print(
            "Products",
            ["Id", "Code", "Name", "Price", "QuantityType", "Stock"],
            [.. products.Select(p => new[] { ConsoleTable.Number(p.Id), p.Code, p.Name, ConsoleTable.Money(p.Price), p.QuantityType, ConsoleTable.Number(p.Stock) })]);
    }

    private static async Task ShowCustomersAsync(IServiceProvider services)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        IReadOnlyList<CustomerRow> customers = await scope.ServiceProvider.GetRequiredService<IShopQuery>().GetCustomersAsync(CancellationToken.None);

        ConsoleTable.Print(
            "Customers",
            ["Id", "Code", "Name"],
            [.. customers.Select(c => new[] { ConsoleTable.Number(c.Id), c.Code, c.Name })]);
    }

    private static async Task ShowOrdersAsync(IServiceProvider services)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        IReadOnlyList<OrderRow> orders = await scope.ServiceProvider.GetRequiredService<IShopQuery>().GetOrdersAsync(CancellationToken.None);

        ConsoleTable.Print(
            "Order",
            ["Id", "Date (UTC)", "Customer", "DeliveryAddress", "Total"],
            [.. orders.Select(o => new[] { ConsoleTable.Number(o.Id), ConsoleTable.Date(o.Date), $"{o.CustomerName} ({o.CustomerCode})", o.DeliveryAddress, ConsoleTable.Money(o.Total) })]);

        Console.WriteLine();
        ConsoleTable.Print(
            "OrderItem",
            ["Id", "OrderId", "Product", "Quantity", "LineTotal"],
            [.. orders.SelectMany(o => o.Lines.Select(line => new[]
            {
                ConsoleTable.Number(line.Id),
                ConsoleTable.Number(o.Id),
                $"{line.ProductName} ({line.ProductCode})",
                $"{line.Quantity} {line.QuantityType}",
                ConsoleTable.Money(line.LineTotal),
            }))]);
    }
}
