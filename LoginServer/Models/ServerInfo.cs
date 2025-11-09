using LoginServer.ODM;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Hzy.Model
{
    [Serializable]
    public class ServerInfo:DBEntity
    {
        [BsonElement("name")]
        public  string Name { get; set; }
        [BsonElement("ip")]
        public  string Ip { get; set; }
        [BsonElement("port")]
        public  string Port { get; set; }
        [BsonElement("server_id")]
        public  int ServerId { get; set; }
        [BsonElement("channel")]
        public  string Channel { get; set; }
        [BsonElement("plat")]
        public  string Plat { get; set; }
        [BsonElement("state")]
        public  int State { get; set; }
        [BsonElement("open_time")]
        public  string OpenTime { get; set; }
        [BsonElement("is_new")]
        public  int IsNew { get; set; }
        [BsonElement("server_version")]
        public  string ServerVersion { get; set; }

        [BsonConstructor]
        public ServerInfo(string name,string ip,string port,int serverId,string channel,string plat,int state,string openTime,int isNew,string serverVersion)
        {
            Name = name;
            Ip = ip;
            Port = port;
            ServerId = serverId;
            Channel = channel;
            Plat = plat;
            State = state;
            OpenTime = openTime;
            IsNew = isNew;
            ServerVersion = serverVersion;
        }
    }
}