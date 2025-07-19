namespace ReservationService.Interfaces;

public interface IReservationService
{
    public Task<bool> ReserveAsync(Guid productId, int quantity);
}
