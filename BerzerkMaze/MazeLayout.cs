using Microsoft.Xna.Framework;

namespace BerzerkMaze;

internal static class MazeLayout
{
    public const int LogicalWidth = 160;
    public const int LogicalHeight = 192;

    public const int PlayfieldTop = 8;
    public const int PlayfieldHeight = 176;

    public const int Brick = 5;
    public const int Wall = 31;

    public const int RoomWidth = Wall * 5 + Brick;   // 160
    public const int RoomHeight = Wall * 3 + Brick;  // 98

    public const int RoomOriginX = (LogicalWidth - RoomWidth) / 2; // 0
    public const int RoomOriginY = PlayfieldTop + (PlayfieldHeight - RoomHeight) / 2; // 47

    public static readonly Point[] PillarPoints =
    [
        new(RoomOriginX + Wall, RoomOriginY + Wall),
        new(RoomOriginX + Wall * 2, RoomOriginY + Wall),
        new(RoomOriginX + Wall * 3, RoomOriginY + Wall),
        new(RoomOriginX + Wall * 4, RoomOriginY + Wall),
        new(RoomOriginX + Wall, RoomOriginY + Wall * 2),
        new(RoomOriginX + Wall * 2, RoomOriginY + Wall * 2),
        new(RoomOriginX + Wall * 3, RoomOriginY + Wall * 2),
        new(RoomOriginX + Wall * 4, RoomOriginY + Wall * 2)
    ];
}
