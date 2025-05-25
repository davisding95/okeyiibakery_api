namespace cakeshop_api.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    public class Cake
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("CakeName")]
        public string CakeName { get; set; } = string.Empty;

        [BsonElement("CakeDescription")]
        public string CakeDescription { get; set; } = string.Empty;

        [BsonElement("CakeImages")]
        public List<string> CakeImages { get; set; } = new();

        [BsonElement("CakeOptions")]
        public List<CakeOption> CakeOptions { get; set; } = new();

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("IsPromoted")]
        public bool IsPromoted { get; set; } = false;

        [BsonElement("IsAvailable")]
        public bool IsAvailable { get; set; } = false;

        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; } = string.Empty;
    }

    public class CakeOption
    {
        [BsonElement("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }
    }
}