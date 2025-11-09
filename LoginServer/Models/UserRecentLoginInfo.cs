namespace Hzy.Model.User
{
    public class UserRecentLoginInfo
    {
        private long LoginTime { get; set; }
        private int Level { get; set; }
        private int Uid { get; set; }
        private string Name { get; set; }
        private int Head { get; set; }
        private int HeadFream { get; set; }

        public UserRecentLoginInfo(long loginTime, int level, int uid, string name, int head, int headFream)
        {
            LoginTime = loginTime;
            Level = level;
            Uid = uid;
            Name = name;
            Head = head;
            HeadFream = headFream;
        }

    }
}
