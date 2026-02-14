namespace BerzerkMaze;

internal enum RobotMissileDirection
{
    None = 0,
    Right = 1,
    Left = 2,
    Down = 4,
    Up = 8
}

internal sealed class RobotMissileState
{
    public bool Active { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public RobotMissileDirection Direction { get; set; }
    public byte DelayAccumulator { get; set; }
    public int FlightTicks { get; set; }
}
