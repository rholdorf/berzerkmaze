namespace BerzerkMaze;

internal sealed class PlayerState
{
    public int X { get; set; }
    public int Y { get; set; }
    public byte FacingMask { get; set; }
    public bool IsMoving { get; set; }
    public int WalkFrame { get; set; }
    public bool IsDying { get; set; }
    public int RespawnTimer { get; set; }
}
