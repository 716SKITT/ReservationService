using ReservationService.Models;
namespace ReservationService.Interfaces;

public interface IReservationService
{
    public Task<Product> AddProductAsync(string name, int stock);
    public Task<bool> ReserveAsync(Guid productId, int quantity);
}