using System.Data.Common;
using System.Data.Entity;

namespace PeanutButter.FluentMigrator.Tests.Shared
{
    public class MooContext : DbContext
    {
        public virtual IDbSet<Cow> Cows { get; set; }
        public MooContext(
            DbConnection connection
        ) : base(connection, true)
        {
        }

        static MooContext()
        {
            Database.SetInitializer<MooContext>(null);
        }
    }
}