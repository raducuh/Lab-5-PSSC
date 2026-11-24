using System.Text;
using Magazin.ConsoleApp;
using Magazin.Data;
using Magazin.Domain.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Consola Windows nu pornește implicit pe UTF-8: fără asta, diacriticele (ă, â, î, ș, ț) s-ar afișa
// greșit la scriere și s-ar citi greșit la introducerea lor de la tastatură.
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Host-ul generic configurează logging-ul și citește appsettings.json / dotnet user-secrets.
// Pentru a vedea comenzile SQL generate de EF Core, setați în appsettings.json nivelul
// "Microsoft.EntityFrameworkCore.Database.Command" la "Information".
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddShopData(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Lipsește ConnectionStrings:DefaultConnection (appsettings.json sau dotnet user-secrets)."));

// Orașele de livrare sunt configurație (nu date din baza de date) și ajung în workflow ca valoare.
builder.Services.AddSingleton(new DeliveryZone(
    new HashSet<string>(
        builder.Configuration.GetSection("Delivery:Cities").GetChildren().Select(city => city.Value).OfType<string>(),
        StringComparer.OrdinalIgnoreCase)));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<PlaceOrderWorkflow>();

using IHost host = builder.Build();

await using (AsyncServiceScope scope = host.Services.CreateAsyncScope())
{
    if (!await scope.ServiceProvider.GetRequiredService<ShoppingContext>().Database.CanConnectAsync())
    {
        Console.WriteLine("Nu mă pot conecta la baza de date Magazin.");
        Console.WriteLine("Verificați că SQL Server rulează, că ați rulat SQL/create-magazin-db.sql și că ConnectionStrings:DefaultConnection este corect.");
        return;
    }
}

await ShopMenu.RunAsync(host.Services);
