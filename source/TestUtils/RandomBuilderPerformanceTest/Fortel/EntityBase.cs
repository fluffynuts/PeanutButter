using System;
using MongoDB.Bson;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class EntityBase
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedUsername { get; set; }
        public DateTime DateLastModified { get; set; }
        public string LastModifiedUsername { get; set; }
        public BsonDocument ExtraElements { get; set; }
    }
}
