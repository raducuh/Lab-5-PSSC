using Magazin.Domain.Events;
using Magazin.Domain.States;

namespace Magazin.Domain.Operations;

/// <summary>Încheierea logicii pure: comanda primește momentul plasării, apoi (după salvare) devine eveniment.</summary>
public static class OrderPlacing
{
    extension(Order.Priced order)
    {
        /// <summary>
        /// Produce starea finală a comenzii, gata de salvat. Funcția este totală și pură — ceasul (<paramref name="placedAt"/>)
        /// vine ca parametru, nu din <c>DateTime.Now</c>.
        /// </summary>
        public Order.Placed Place(DateTimeOffset placedAt) =>
            new(order.Customer, order.Lines, order.DeliveryAddress, order.Total, placedAt);
    }

    extension(Order.Placed order)
    {
        /// <summary>
        /// Convertește starea finală în evenimentul anunțat celorlalte contexte, după ce baza de date a atribuit
        /// <paramref name="orderId"/> la salvare.
        /// </summary>
        public OrderPlacedEvent ToEvent(int orderId) =>
            new(orderId, order.Customer, order.Lines, order.DeliveryAddress, order.Total, order.PlacedAt);
    }
}
