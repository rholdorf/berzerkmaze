namespace BerzerkMaze;

internal sealed class PlayerMissileState
{
    public bool Active { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public byte DirectionMask { get; set; }
    public byte DelayAccumulator { get; set; }
}
