using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class GenericCosmosStore<T> : DbContext, IGenericStore<T>
        where T : BaseEntity
    {
        public DbSet<T> Dataset { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(
                $"AccountEndpoint={DbHost};AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                DbName,
                opts =>
                {
                    opts.HttpClientFactory(
                        () =>
                            new HttpClient(
                                new HttpClientHandler()
                                {
                                    ServerCertificateCustomValidationCallback =
                                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                                }
                            )
                    );
                    opts.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Gateway);
                    opts.LimitToEndpoint(true);
                }
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<T>(entity =>
            {
                entity.ToContainer(EntityName);
            });
        }

        private string DbName { get; }
        private string DbHost { get; }
        private string EntityName { get; }

        public GenericCosmosStore(string dbHost, string dbName, string entityName)
        {
            DbHost = dbHost;
            DbName = dbName;
            EntityName = entityName;
        }

        public void Setup()
        {
            _ = Database.EnsureCreatedAsync().Result;
        }

        public void Clear()
        {
            var entities = Dataset.Where(entity => true).ToListAsync().Result;
            foreach (var e in entities)
            {
                Dataset.Remove(e);
            }

            _ = SaveChangesAsync().Result;
        }

        public async Task<long> Count()
        {
            return await Dataset.CountAsync();
        }

        public async Task<bool> Delete(string id)
        {
            T? entity = await Dataset.FindAsync(id);
            if (entity is null)
                return false;

            Dataset.Remove(entity);
            var cnt = await SaveChangesAsync();
            return cnt == 1;
        }

        public async Task<T?> Find(string id)
        {
            return await Dataset.FindAsync(id);
        }

        public async Task<T?> FindBy(Expression<Func<T, bool>> expr)
        {
            return await Dataset.Where(expr).FirstOrDefaultAsync();
        }

        public async Task<T> Insert(T entity)
        {
            await Dataset.AddAsync(entity);
            await SaveChangesAsync();

            return entity;
        }

        public async Task<List<T>> ReadAll()
        {
            var list = await Dataset.Where(e => true).ToListAsync();

            return list;
        }

        public async Task<T> Update(T entity)
        {
            entity.Version += 1;

            Dataset.Update(entity);
            await SaveChangesAsync();

            return entity;
        }
    }
}
