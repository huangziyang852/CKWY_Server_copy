namespace IGrains.GrainState;

public class WorldInfo
{
    public List<World> worlds = new List<World>();
    public List<Stage> stages = new List<Stage>();

    public class World
    { 
        public int worldId { get; set; }
       
       public World(int worldId)
       {
           this.worldId = worldId;
       }
    }

    public class Stage
    {
        public int stageId { get; set; }

        public Stage(int stageId)
        {
            this.stageId = stageId;
        }
    }
}