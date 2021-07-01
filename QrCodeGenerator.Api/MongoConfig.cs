namespace QrCodeGenerator.Api
{
    public class MongoConfig
    {
        public string ConnectionString { get; set; }
        public bool EnableQuickMongoConnectionCycle { get; set; }
        public string Database { get; set; }
        public string Employees { get; set; }
    }
}
