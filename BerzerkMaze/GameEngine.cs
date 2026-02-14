using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BerzerkMaze;

internal class GameEngine
{
    // http://www.robotron2084guidebook.com/home/games/berzerk/mazegenerator/map/

    private int _roomNumber;
    private PillarDirection[] _directions;
    private readonly Renderer _renderer = new();

    public GameEngine()
    {
        _roomNumber = BerzerkMaze.Calculate(_roomNumber);
        UpdateDirections();
    }

    private void UpdateDirections()
    {
        _directions = BerzerkMaze.GenerateRoom(_roomNumber);
    }

    public void Render(SpriteBatch spriteBatch, Texture2D pixelTexture, Point offset)
    {
        _renderer.Render(spriteBatch, pixelTexture, _directions, offset);
    }

    public void Up()
    {
        _roomNumber -= 32;
        UpdateDirections();
    }

    public void Left()
    {
        _roomNumber--;
        UpdateDirections();
    }

    public void Right()
    {
        _roomNumber++;
        UpdateDirections();
    }

    public void Down()
    {
        _roomNumber += 32;
        UpdateDirections();
    }
}