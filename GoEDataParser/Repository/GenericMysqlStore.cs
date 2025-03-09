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
            Dataset.ExecuteDelete();
            SaveChanges();
        }

        public long Count()
        {
            return Dataset.Count();
        }

        public bool Delete(string id)
        {
            T? entity = Dataset.Find(id);
            if (entity is null) return false;
            
            Dataset.Remove(entity);
            var cnt = this.SaveChanges();
            return cnt == 1;
        }

        public T? Find(string id)
        {
            return Dataset.Find(id);
        }

        public T? FindBy<TV>(string key, TV value)
        {
            string q = $"SELECT * FROM {DbTablename} WHERE {key} = '{value}'";

            return Dataset.FromSqlRaw(q).ToList().FirstOrDefault();
        }

        public T Insert(T entity)
        {
            Dataset.Add(entity);
            SaveChanges();

            return entity;
        }

        public List<T> ReadAll()
        {
            var list = Dataset.Where(e => true).ToList();

            return list;
        }

        public T Update(T entity)
        {
            entity.Version += 1;
            
            Dataset.Update(entity);
            SaveChanges();

            return entity;
        }
    }
}
