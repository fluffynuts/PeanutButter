using System.ComponentModel.DataAnnotations;

namespace PeanutButter.FluentMigrator.Tests.Shared
{
    public class Cow
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string MooDialect { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsDairy { get; set; }
        public virtual bool IsLactoseIntolerant { get; set; }
    }
}