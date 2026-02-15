using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MySql.Data.MySqlClient;

namespace Repository
{
    public class GenericMysqlStore<T> : DbContext, IGenericStore<T>
        where T : BaseEntity
    {
        public DbSet<T> Dataset { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(
                $"server={DbHost};database={DbName};user={DbUser};password={DbPassword}"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<T>(entity =>
            {
                entity.ToTable(DbTablename);
            });
        }

        private string DbTablename { get; }
        private string DbHost { get; }
        private string DbName { get; }
        private string DbUser { get; }
        private string DbPassword { get; }

        public GenericMysqlStore(
            string dbHost,
            string dbName,
            string dbTablename,
            string dbUser,
            string dbPassword
        )
        {
            DbTablename = dbTablename;
            DbHost = dbHost;
            DbName = dbName;
            DbUser = dbUser;
            DbPassword = dbPassword;
        }

        public void Setup()
        {
            Database.EnsureCreated();

            try
            {
                var databaseCreator = this.GetService<IRelationalDatabaseCreator>();
                databaseCreator.CreateTables();
            }
            catch (MySqlException)
            {
                // nothing
            }
        }

        public void Clear()
        {
            _ = Dataset.ExecuteDeleteAsync().Result;
            _ = SaveChangesAsync().Result;
        }

        public async Task<long> Count()
        {
            return await Dataset.CountAsync();
        }

        public async Task<bool> Delete(string id)
        {
            T? entity = Dataset.Find(id);
            if (entity is null)
                return false;

            Dataset.Remove(entity);
            var cnt = await this.SaveChangesAsync();
            return cnt == 1;
        }

        public async Task<T?> Find(string id)
        {
            return await Dataset.FindAsync(id);
        }

        public async Task<T?> FindBy(Expression<Func<T, bool>> expr)
        {
            return (await Dataset.Where(expr).ToListAsync()).FirstOrDefault();
        }

        public async Task<T> Insert(T entity)
        {
            Dataset.Add(entity);
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
