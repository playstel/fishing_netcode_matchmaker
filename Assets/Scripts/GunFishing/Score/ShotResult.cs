namespace GunFishing.Score
{
    public class ShotResult
    {
        public int score;             
        public string fishName;       

        public ShotResult(int score, string fishName)
        {
            this.score = score;
            this.fishName = fishName;
        }
    }
}