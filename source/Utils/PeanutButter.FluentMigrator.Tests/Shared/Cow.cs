using System.ComponentModel.DataAnnotations;

namespace PeanutButter.FluentMigrator.Tests.Shared
{
    public interface ICow
    {
        string MooDialect { get; set; }
        string Name { get; set; }
        bool IsDairy { get; set; }
        bool IsLactoseIntolerant { get; set; }
    }

    public class Cow: ICow
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string MooDialect { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsDairy { get; set; }
        public virtual bool IsLactoseIntolerant { get; set; }
    }

}