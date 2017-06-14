namespace RandomBuilderPerformanceTest.Fortel
{
    public class Log : EntityBase
    {
        public dynamic Before { get; set; }
        public dynamic After { get; set; }
        public OperationType OperationType { get; set; }
        public string ObjectType { get; set; }
    }
}