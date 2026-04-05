namespace Matchmancer.Core
{
    public class Tile
    {
        public TileType Type { get; set; }
        public SigilType Sigil { get; set; }
        public GridPosition Position { get; set; }
        public bool IsEmpty => Type == TileType.None && Sigil == SigilType.None;
        public bool IsSigil => Sigil != SigilType.None;

        public Tile(TileType type, GridPosition position)
        {
            Type = type;
            Sigil = SigilType.None;
            Position = position;
        }

        public void Clear()
        {
            Type = TileType.None;
            Sigil = SigilType.None;
        }
    }
}
