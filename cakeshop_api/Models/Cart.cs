
using MongoDB.Bson.Serialization.Attributes;

namespace cakeshop_api.Models
{
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }

        [BsonElement("cakeId")]
        public required string CakeId { get; set; }

        [BsonElement("cakeName")]
        public required string CakeName { get; set; }

        [BsonElement("cakeImage")]
        public required string CakeImage { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("cakePrice")]
        public required decimal CakePrice { get; set; }

        [BsonElement("cakeSize")]
        public required string CakeSize { get; set; }

        [BsonElement("optionId")]
        public required string OptionId { get; set; }

    }
}