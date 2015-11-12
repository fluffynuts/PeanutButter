using System;

namespace PeanutButter.Utils.Entity
{
    public abstract class EntityBase
    {
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
        public bool Enabled { get; set; }

        public EntityBase()
        {
            Enabled = true;
        }
    }
}
