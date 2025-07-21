using Microsoft.EntityFrameworkCore;
using ReservationService.Infrastructure;
using Testcontainers.PostgreSql;

namespace ReservationService.Tests.TestContainer
{
    public sealed class DatabaseInitializer : DatabaseInitializerFixture, IClassFixture<DatabaseInitializerFixture>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithImage("postgres:14.7")
            .WithDatabase("reservation_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .WithName("reservation_pg_test")
            .Build();

        public ReservationDbContext DbContext { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            var options = CreateOptions();

            DbContext = new ReservationDbContext(options);
            await DbContext.Database.EnsureCreatedAsync();
        }

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        public override DbContextOptions<ReservationDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<ReservationDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;
        }

        public override string GetConnectionString() => _container.GetConnectionString();
    }
}
