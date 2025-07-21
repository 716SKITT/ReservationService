using Microsoft.EntityFrameworkCore;
using ReservationService.Infrastructure;
using ReservationService.Interfaces;
using ReservationService.Models;

namespace ReservationService.Services;

public class ProductReservationService : IReservationService
{
    private readonly ReservationDbContext _context;

    public ProductReservationService(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Product> AddProductAsync(string name, int stock)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name required");
        if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock));

        var product = new Product(name, stock);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<bool> ReserveAsync(Guid productId, int quantity)
    {
        if (quantity <= 0) return false;

        var query = _context.Products
        .FromSqlRaw(
            @"UPDATE ""Products"" 
            SET ""Stock"" = ""Stock"" - {0}
            WHERE ""Id"" = {1} AND ""Stock"" >= {0}
            RETURNING *",
            quantity, productId);

        var updatedProduct = (await query.ToListAsync()).FirstOrDefault();

        if (updatedProduct == null)
            return false;

        var reservation = new Reservation(quantity, updatedProduct);
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return true;
    }


}
