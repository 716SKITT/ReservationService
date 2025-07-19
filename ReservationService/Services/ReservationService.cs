using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using ReservationService.Infrastructure;
using ReservationService.Interfaces;
using ReservationService.Models;

namespace ReservationService.Services;

public class ReservationService : IReservationService
{
    private readonly ReservationDbContext _context;
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public ReservationService(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ReserveAsync(Guid productId, int quantity)
    {
        var semaphore = _locks.GetOrAdd(productId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            Product? product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null || product.Stock < quantity)
                return false;

            product.TryReserve(quantity);

            Reservation reservation = new Reservation(quantity, product);
            await _context.Reservations.AddAsync(reservation);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }
        finally
        {
            semaphore.Release();
        }
    }
}