using LoginServer.ODM;
using MongoDB.Bson.Serialization.Attributes;

namespace Hzy.Model.User
{
    [Serializable]
    public class UserRegisterInfo:DBEntity
    {
        [BsonElement("account")]
        public string Account {  get; set; }
        [BsonElement("password")]
        public string Password {  get; set; }
        [BsonElement("openId")]
        public long OpenId { get; set; }

        [BsonConstructor]
        public UserRegisterInfo(string account, string password,long openId)
        {
            Account = account;
            Password = password;
            OpenId = openId;
        }
    }
}