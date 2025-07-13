namespace Test.Worker
{
    internal static class Utils
    {
        public static Dictionary<string, (Type, Type)> GetConsumerMappings()
        {
            return new Dictionary<string, (Type, Type)>
            {
                ["SubmitOrder"] = (typeof(SubmitOrderConsumer), typeof(SubmitOrderConsumerDefinition)),
                ["ShipmentCreated"] = (typeof(ShipmentCreatedConsumer), typeof(ShipmentCreatedConsumerDefinition))
            };
        }
    }
}