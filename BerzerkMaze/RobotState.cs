namespace BerzerkMaze;

internal enum RobotDirection
{
    Up,
    Down,
    Left,
    Right
}

internal sealed class RobotState
{
    public int X { get; set; }
    public int Y { get; set; }
    public int IdleAnimIndex { get; set; }
    public RobotDirection Direction { get; set; }
    public int WalkFrame { get; set; }
    public bool IsMoving { get; set; }
    public int DecisionCooldown { get; set; }
    public bool IsDying { get; set; }
    public int DeathFrame { get; set; }
}
