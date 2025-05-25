using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace cakeshop_api.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("CategoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }
}