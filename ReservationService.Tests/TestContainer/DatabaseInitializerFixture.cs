using Microsoft.EntityFrameworkCore;
using ReservationService.Infrastructure;

namespace ReservationService.Tests.TestContainer
{
    public abstract class DatabaseInitializerFixture
    {
        private readonly Lazy<DbContextOptions<ReservationDbContext>> _dataContextOptions;

        public DbContextOptions<ReservationDbContext> DataContextOptions => _dataContextOptions.Value;

        protected DatabaseInitializerFixture()
        {
            _dataContextOptions = new Lazy<DbContextOptions<ReservationDbContext>>(InitializeOptions);
        }

        public abstract DbContextOptions<ReservationDbContext> CreateOptions();

        public abstract string GetConnectionString();

        private DbContextOptions<ReservationDbContext> InitializeOptions()
        {
            var options = CreateOptions();

            var factory = new ReservationDbContextFactory(GetConnectionString());

            for (int attempt = 0; attempt < 3; attempt++)
            {
                using var context = factory.Create();
                context.Database.EnsureCreated();
            }

            return options;
        }
    }
}
