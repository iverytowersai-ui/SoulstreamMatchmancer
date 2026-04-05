namespace Matchmancer.Core
{
    public enum SigilType
    {
        None = 0,
        Line,  // Match 4 in a straight line — clears row OR column
        Star,  // Match 5 in a straight line — clears all of one chosen type
        Nova   // L-shaped or T-shaped match — 3x3 explosion
    }
}
