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

        public long Count()
        {
            return Dataset.CountAsync().Result;
        }

        public bool Delete(string id)
        {
            T? entity = Dataset.FindAsync(id).Result;
            if (entity is null)
                return false;

            Dataset.Remove(entity);
            var cnt = this.SaveChangesAsync().Result;
            return cnt == 1;
        }

        public T? Find(string id)
        {
            return Dataset.FindAsync(id).Result;
        }

        public T? FindBy(Expression<Func<T, bool>> expr)
        {
            return Dataset.Where(expr).ToListAsync().Result.FirstOrDefault();
        }

        public T? FindBy<TV>(string key, TV value)
        {
            throw new NotImplementedException("Please use expression FindBy instead");
        }

        public T Insert(T entity)
        {
            _ = Dataset.AddAsync(entity).Result;
            _ = SaveChangesAsync().Result;

            return entity;
        }

        public List<T> ReadAll()
        {
            var list = Dataset.Where(e => true).ToListAsync().Result;

            return list;
        }

        public T Update(T entity)
        {
            entity.Version += 1;

            Dataset.Update(entity);
            _ = SaveChangesAsync().Result;

            return entity;
        }
    }
}
