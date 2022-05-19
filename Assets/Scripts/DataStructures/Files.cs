public struct ApplicationData
{
    public int lastPlayed;
    public bool firstStart;//true
}
public struct Save
{
    public bool empty;//true
    public float percentage;
    public string lastPlay;
    public int lastMission;
    public int lastRegion;
    public Difficulty difficulty;
}
public struct PlayerData
{
    public int region;
    public int shillings;

    //Items
    public int[] itemIDs;
    public int[] itemLocsX;
    public int[] itemLocsY;
    public int[] itemCounts;

    //Weapons
    public int slot;
    public int holsterSize;
    public string[] wpnKeys;
    public int ammoSize;
    public int[] Ammo { get; set; }
    public int[] gadgetsSize;
    public int[] gadgets;

    public bool firstStart;
    public int[] locationStatus;
    public int storyIndex;

    public string lastLocation;

    public int[][] unlockedShopIDs;

    public float[] strengths;
    public string[] abilities;
    public int abilityPoints;
}
