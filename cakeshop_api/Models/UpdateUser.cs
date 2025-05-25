using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cakeshop_api.Models
{
    public class UpdateUser

    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }
    }
}