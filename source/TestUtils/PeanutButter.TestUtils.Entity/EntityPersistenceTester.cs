namespace PeanutButter.TestUtils.Entity
{
    public static class EntityPersistenceTester
    {
        public static EntityPersistenceTesterFor<T> CreateFor<T>() where T : class
        {
            return new EntityPersistenceTesterFor<T>();
        }
    }
}