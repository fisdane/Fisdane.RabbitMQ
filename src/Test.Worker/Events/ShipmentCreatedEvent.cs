public class ShipmentCreatedEvent
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; }
    public string ShippingAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
