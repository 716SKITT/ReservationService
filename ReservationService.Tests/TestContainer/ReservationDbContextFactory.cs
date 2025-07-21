using Microsoft.EntityFrameworkCore;
using ReservationService.Infrastructure;

namespace ReservationService.Tests.TestContainer
{
    public class ReservationDbContextFactory
    {
        private readonly string _connectionString;

        public ReservationDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ReservationDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ReservationDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            return new ReservationDbContext(options);
        }
    }
}
