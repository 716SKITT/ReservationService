using Microsoft.EntityFrameworkCore;
using ReservationService.Services;
using ReservationService.Tests.TestContainer;

namespace ReservationService.Tests
{
    public class ReservationStressTests : IClassFixture<DatabaseInitializer>
    {
        private readonly DatabaseInitializer _db;
        private const int InitialStock = 100;
        private const int ThreadCount = 10;
        private const int AttemptsPerThread = 100;
        private const int MaxReserveQuantity = 3;

        public ReservationStressTests(DatabaseInitializer db)
        {
            _db = db;
        }

        [Fact]
        public async Task High_Load_Reservation_Should_Not_Overbook()
        {
            var factory = new ReservationDbContextFactory(_db.GetConnectionString());
            Guid productId;

            using (var context = factory.Create())
            {
                var service = new ProductReservationService(context);
                var product = await service.AddProductAsync("Stress Test Product", InitialStock);
                productId = product.Id;
            }

            var totalReserved = 0;
            var tasks = new List<Task<List<ReservationAttempt>>>();
            var random = new Random();
            var startSignal = new ManualResetEventSlim(false);

            for (int i = 0; i < ThreadCount; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(async () =>
                {
                    var attempts = new List<ReservationAttempt>();
                    using var context = factory.Create();
                    var service = new ProductReservationService(context);

                    startSignal.Wait();

                    for (int j = 0; j < AttemptsPerThread; j++)
                    {
                        var quantity = random.Next(1, MaxReserveQuantity + 1);
                        var startTime = DateTime.UtcNow;
                        var success = await service.ReserveAsync(productId, quantity);
                        var duration = DateTime.UtcNow - startTime;

                        attempts.Add(new ReservationAttempt
                        {
                            ThreadId = threadId,
                            AttemptNumber = j,
                            Quantity = quantity,
                            IsSuccess = success,
                            Duration = duration
                        });

                        if (success)
                        {
                            Interlocked.Add(ref totalReserved, quantity);
                        }
                    }

                    return attempts;
                }));
            }

            startSignal.Set();
            var allAttempts = (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();

            using (var assertContext = factory.Create())
            {
                var finalStock = await assertContext.Products
                    .Where(p => p.Id == productId)
                    .Select(p => p.Stock)
                    .FirstAsync();

                var totalReservedFromDb = await assertContext.Reservations
                    .Where(r => r.ProductId == productId)
                    .SumAsync(r => r.Quantity);

                Assert.True(finalStock >= 0, "Stock should never be negative");
                Assert.Equal(InitialStock, finalStock + totalReservedFromDb);

                var successfulAttempts = allAttempts.Where(a => a.IsSuccess).ToList();
                var failedAttempts = allAttempts.Where(a => !a.IsSuccess).ToList();

                Console.WriteLine($"Total successful reservations: {successfulAttempts.Count}");
                Console.WriteLine($"Total failed attempts: {failedAttempts.Count}");
                Console.WriteLine($"Average duration (success): {successfulAttempts.Average(a => a.Duration.TotalMilliseconds):F2} ms");
                Console.WriteLine($"Average duration (failed): {failedAttempts.Average(a => a.Duration.TotalMilliseconds):F2} ms");

                var successByThread = successfulAttempts
                    .GroupBy(a => a.ThreadId)
                    .OrderBy(g => g.Key)
                    .Select(g => new { Thread = g.Key, SuccessCount = g.Count() })
                    .ToList();

                Console.WriteLine("Success distribution by thread:");
                foreach (var item in successByThread)
                {
                    Console.WriteLine($"Thread {item.Thread}: {item.SuccessCount} successful reservations");
                }

                var orderedSuccesses = successfulAttempts
                    .OrderBy(a => a.Duration)
                    .Take(10)
                    .ToList();

                Console.WriteLine("Fastest successful reservations:");
                foreach (var attempt in orderedSuccesses)
                {
                    Console.WriteLine($"Thread {attempt.ThreadId}, attempt {attempt.AttemptNumber}: {attempt.Duration.TotalMilliseconds:F2} ms");
                }
            }
        }
    }

    public class ReservationAttempt
    {
        public int ThreadId { get; set; }
        public int AttemptNumber { get; set; }
        public int Quantity { get; set; }
        public bool IsSuccess { get; set; }
        public TimeSpan Duration { get; set; }
    }
}