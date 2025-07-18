namespace ReservationService.Models;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }

    public Reservation(int quantity, Product product)
    {
        if (quantity <= 0)
            throw new ArgumentException("quantity must be positive");

        Id = Guid.NewGuid();
        Quantity = quantity;
        Product = product;
        ProductId = product.Id;
    }

    private Reservation() { }
}