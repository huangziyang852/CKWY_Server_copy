using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LoginServer.ODM
{
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class DBEntity
    {
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string ID { get; set; }
        public DBEntity()
        {
            ID = ObjectId.GenerateNewId().ToString();
        }

    }
}
