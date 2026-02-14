namespace BerzerkMaze;

internal static class Program
{
    private static void Main()
    {
        using var game = new MazeGame();
        game.Run();
    }
}