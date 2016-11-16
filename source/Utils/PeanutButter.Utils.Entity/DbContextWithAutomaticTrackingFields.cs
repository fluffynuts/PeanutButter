using System;
using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PeanutButter.Utils.Entity
{
    public class DbContextWithAutomaticTrackingFields : DbContext
    {
        public DbContextWithAutomaticTrackingFields(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbContextWithAutomaticTrackingFields(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
        }

        public DbContextWithAutomaticTrackingFields(DbConnection connection)
            : base(connection, false)
        {
        }


        public override int SaveChanges()
        {
            PerformAutoTrackingLogic();
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync()
        {
            PerformAutoTrackingLogic();
            try
            {
                return await base.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void PerformAutoTrackingLogic()
        {
            var autoEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity as EntityBase)
                .Where(e => e != null)
                .ToArray();
            var tooEarly = new DateTime(1900, 1, 1);
            var currentTime = DateTime.Now;
            autoEntities.ForEach(e =>
            {
                if (e.Created < tooEarly)
                {
                    e.Created = currentTime;
                    e.LastModified = null;
                }
                else
                    e.LastModified = currentTime;
            });
        }
    }
}