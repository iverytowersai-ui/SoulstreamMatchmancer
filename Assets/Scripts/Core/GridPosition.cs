namespace Matchmancer.Core
{
    public struct GridPosition
    {
        public int Row;
        public int Col;

        public GridPosition(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool IsAdjacentTo(GridPosition other)
        {
            int dr = System.Math.Abs(Row - other.Row);
            int dc = System.Math.Abs(Col - other.Col);
            return (dr + dc) == 1;
        }

        public override string ToString() => $"({Row},{Col})";

        public override bool Equals(object obj) =>
            obj is GridPosition other && Row == other.Row && Col == other.Col;

        public override int GetHashCode() => Row * 100 + Col;

        public static bool operator ==(GridPosition a, GridPosition b) => a.Row == b.Row && a.Col == b.Col;
        public static bool operator !=(GridPosition a, GridPosition b) => !(a == b);
    }
}
