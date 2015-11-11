using System;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;

namespace EntityUtilities
{
    public class DbContextWithAutomaticTrackingFields : DbContext
    {
        public DbContextWithAutomaticTrackingFields(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbContextWithAutomaticTrackingFields(DbConnection connection, bool contextOwnsConnection)
            :base(connection, contextOwnsConnection)
        {
        }

        public DbContextWithAutomaticTrackingFields(DbConnection connection)
            :base(connection, false)
        {
        }


        public override int SaveChanges()
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
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static DateTime _minDate = new DateTime(1900, 1, 1);

        private static PropertyInfo[] SetTimestampsOn(object entity, DateTime currentTime)
        {
            var propertyInfos = entity.GetType().GetProperties();
            var createdPropInfo = propertyInfos.FirstOrDefault(pi => pi.Name == "Created");
            if (createdPropInfo != null)
            {
                var currentValue = (DateTime)createdPropInfo.GetValue(entity);
                if (currentValue < _minDate)
                    createdPropInfo.SetValue(entity, currentTime);
            }
            var propInfo = propertyInfos.FirstOrDefault(pi => pi.Name == "LastModified");
            if (propInfo != null)
            {
                propInfo.SetValue(entity, currentTime);
            }
            return propertyInfos;
        }
    }
}