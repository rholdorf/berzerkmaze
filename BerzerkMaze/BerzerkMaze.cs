
namespace BerzerkMaze;

/* http://www.robotron2084guidebook.com/home/games/berzerk/mazegenerator/code/ */
internal static class BerzerkMaze
{
    public static PillarDirection[] GenerateRoom(int roomNumber)
    {
        var ret = new PillarDirection[8];
        var value = roomNumber;

        for (var i = 0; i < 8; i++)
        {
            value *= 7;
            value += 0x3153;
            value *= 7;
            value += 0x3153;
            ret[i] = DetermineDirection(value);
        }

        return ret;
    }

    public static int Calculate(int value)
    {
        value *= 7;
        value += 0x3153;
        return value;
    }

    private static PillarDirection DetermineDirection(int data)
    {
        // 0x0300 em binário equivale a: 0000 0011 0000 0000
        // (00 is N, 01 is S, 10 is E, 11 is W)
        switch (data & 0x0300)
        {
            case 0x0300: // 11
                return PillarDirection.West;

            case 0x0200: // 10
                return PillarDirection.East;

            case 0x0100: // 01
                return PillarDirection.South;
        }

        //case 0x0000: // 00
        return PillarDirection.North;
    }
}