using MongoDB.Bson.Serialization.Attributes;

namespace cakeshop_api.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }

        [BsonElement("OrderItems")]
        public required List<OrderItem> OrderItems { get; set; }

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("orderStatus")]
        public string OrderStatus { get; set; } = Status.Pending;

        [BsonElement("paymentStatus")]
        public bool PaymentStatus { get; set; } = false;
        [BsonElement("paymentIntentId")]
        public string? PaymentIntentId { get; set; }
        [BsonElement("stripeCustomerId")]
        public string? StripeCustomerId { get; set; }
        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; } = 0;
    }

    public class OrderItem
    {
        [BsonElement("cakeId")]
        public required string CakeId { get; set; }

        [BsonElement("cakeName")]
        public required string CakeName { get; set; }

        [BsonElement("cakePrice")]
        public required decimal CakePrice { get; set; }

        [BsonElement("cakeSize")]
        public required string CakeSize { get; set; }

        [BsonElement("cakeImage")]
        public required string CakeImage { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("optionId")]
        public required string OptionId { get; set; }
    }

    public class Status
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Completed = "Completed";
    }
}