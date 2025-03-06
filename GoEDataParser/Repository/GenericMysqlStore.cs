using Microsoft.EntityFrameworkCore;
using Repository;

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
            this.DbTablename = dbTablename;
            this.DbHost = dbHost;
            this.DbName = dbName;
            this.DbUser = dbUser;
            this.DbPassword = dbPassword;

            Database.EnsureCreated();
        }

        ~GenericMysqlStore() { }

        public void Clear()
        {
            Dataset.ExecuteDelete();
        }

        public long Count()
        {
            return Dataset.Count<T>();
        }

        public bool Delete(string id)
        {
            T? entity = Dataset.Find(id);
            if (entity is not null)
            {
                Dataset.Remove(entity);
                return true;
            }
            return false;
        }

        public T? Find(string id)
        {
            return Dataset.Find(id);
        }

        public T? FindBy<TV>(string key, TV value)
        {
            string q = $"SELECT * FROM {DbTablename} WHERE {key} = '{value}'";

            return Dataset.FromSqlRaw(q).ToList().FirstOrDefault<T>();
        }

        public T Insert(T entity)
        {
            Dataset.Add(entity);
            this.SaveChanges();

            return entity;
        }

        public List<T> ReadAll()
        {
            var list = Dataset.Where(e => true).ToList();

            return list;
        }

        public T Update(T entity)
        {
            Dataset.Update(entity);
            this.SaveChanges();

            return entity;
        }
    }
}
