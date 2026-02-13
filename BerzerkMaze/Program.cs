using System;

namespace BerzerkMaze
{
    static class Program
    {
        static void Main()
        {
            using (var game = new MazeGame())
            {
                game.Run();
            }
        }
    }
}
