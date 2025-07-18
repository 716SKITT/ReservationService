namespace ReservationService.Models;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public int Stock { get; private set; }

    public Product(string name, int initStock = 0)
    {
        if (initStock < 0)
            throw new ArgumentException("init Stock must be positive");

        Id = Guid.NewGuid();
        Name = name;
        Stock = initStock;
    }

    public bool TryReserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("quantity must be positive");

        if (Stock < quantity) 
            return false;

        Stock -= quantity;
        return true;
    }

    private Product() { }
}