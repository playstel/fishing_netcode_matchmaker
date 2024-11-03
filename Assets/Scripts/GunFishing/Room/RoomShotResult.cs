namespace GunFishing.Score
{
    public class RoomShotResult
    {
        public int score;             
        public string fishName;       

        public RoomShotResult(int score, string fishName)
        {
            this.score = score;
            this.fishName = fishName;
        }
    }
}