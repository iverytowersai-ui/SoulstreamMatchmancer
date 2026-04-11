namespace Matchmancer.Boosters
{
    /// <summary>
    /// All consumable booster categories available to the player.
    /// Numeric values are stable for save data — never reorder.
    /// </summary>
    public enum BoosterType
    {
        ExtraMoves   = 0, // +N moves to the current battle
        Hammer       = 1, // remove a single tile
        Shuffle      = 2, // reshuffle the entire board
        Bomb         = 3, // clear a 3x3 area
        ColorBlast   = 4, // remove all tiles of a chosen TileType
        Heal         = 5, // restore a chunk of player HP
        EnergySurge  = 6, // instantly fill the ultimate meter
        TimeFreeze   = 7, // skip the next enemy turn
    }
}
