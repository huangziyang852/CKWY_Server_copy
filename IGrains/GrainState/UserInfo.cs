namespace IGrains.GrainState
{
    public class UserInfo
    {
        public string UsertId { get; set; }

        public long OpenId {  get; set; }
        public string Name { get; set; }

        public int Level { get; set; }
        public int Exp {  get; set; }

        public int Gold { get; set; }

        public int Diamond { get; set; }

        public List<BattleSlot> Deck = new List<BattleSlot>();
        
        public UserInfo() {
            UsertId = "";
            OpenId = 0;
            Name = "default";
            Level = 0;
            Exp = 0;
            Gold = 0;
            Diamond = 0;
            Deck = new List<BattleSlot>();
        }

        public UserInfo(string usertId,long openId)
        {
            UsertId = usertId;
            OpenId = openId;
            Name = "default";
            Level = 0;
            Exp = 0;
            Gold = 0;
            Diamond = 0;
            Deck = new List<BattleSlot>();
        }
    }

    public class BattleSlot
    {
        public int position;
        public string heroCd;
    }
}
