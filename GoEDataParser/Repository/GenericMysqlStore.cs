using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MySql.Data;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Repository
{
    public class GenericMysqlStore<T> : DbContext, IGenericStore<T>
        where T : BaseEntity
    {
        public DbSet<T> Dataset { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(
                $"server={dbHost};database={dbName};user={dbUser};password={dbPassword}"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<T>(entity =>
            {
                entity.ToTable(dbTablename);
            });
        }

        private string dbTablename { get; }
        private string dbHost { get; }
        private string dbName { get; }
        private string dbUser { get; }
        private string dbPassword { get; }

        public GenericMysqlStore(
            string dbHost,
            string dbName,
            string dbTablename,
            string dbUser,
            string dbPassword
        )
        {
            this.dbTablename = dbTablename;
            this.dbHost = dbHost;
            this.dbName = dbName;
            this.dbUser = dbUser;
            this.dbPassword = dbPassword;
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

        public T? FindBy<V>(string key, V value)
        {
            string q = $"SELECT * FROM {dbTablename} WHERE {key} = '{value}'";

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
