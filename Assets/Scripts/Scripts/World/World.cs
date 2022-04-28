namespace GameWorld
{
    public static class World
    {
        public static Difficulty Difficulty { get; private set; }

        public static void SetDifficulty(Difficulty difficulty)
        {
            Difficulty = difficulty;
        }
    }
}
