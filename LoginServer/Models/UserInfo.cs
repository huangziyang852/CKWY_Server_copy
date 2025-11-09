using LoginServer.ODM;
using MongoDB.Bson.Serialization.Attributes;

namespace LoginServer.Models
{
    [Serializable]
    public class UserInfo:DBEntity
    {
        [BsonElement("openId")]
        public long OpenId { get; set; }
        [BsonElement("serverId")]
        public int ServerId { get; set; }
        [BsonElement("platform")]
        public string Platform { get; set; }
        public string UId { get; set; }

        [BsonConstructor]
        public UserInfo(long openId, int serverId, string platform, string uId)
        {
            OpenId = openId;
            ServerId = serverId;
            Platform = platform;
            UId = uId;
        }
    }
}
