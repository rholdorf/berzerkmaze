using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BerzerkMaze;

internal class Renderer
{
    private readonly int _brick;
    private readonly int _wall;
    private readonly Point[] _pillarPoints;
    private readonly Color _wallColor = new(11, 36, 251);
    private const int BRICKS_PER_WALL = 12;

    public Renderer()
    {
        _brick = 4;
        _wall = _brick * BRICKS_PER_WALL;

        _pillarPoints = new Point[8];
        _pillarPoints[0] = new Point(_wall, _wall);
        _pillarPoints[1] = new Point(_wall * 2, _wall);
        _pillarPoints[2] = new Point(_wall * 3, _wall);
        _pillarPoints[3] = new Point(_wall * 4, _wall);
        _pillarPoints[4] = new Point(_wall, _wall * 2);
        _pillarPoints[5] = new Point(_wall * 2, _wall * 2);
        _pillarPoints[6] = new Point(_wall * 3, _wall * 2);
        _pillarPoints[7] = new Point(_wall * 4, _wall * 2);
    }

    public void Render(SpriteBatch spriteBatch, Texture2D pixelTexture, PillarDirection[] directions, Point offset)
    {
        // paredes fixas
        {
            // superior
            FillRectangle(spriteBatch, pixelTexture, offset.X + 0, offset.Y + 0, _wall * 2, _brick);
            FillRectangle(spriteBatch, pixelTexture, offset.X + _wall * 3, offset.Y + 0, _wall * 2, _brick);

            // inferior
            FillRectangle(spriteBatch, pixelTexture, offset.X + 0, offset.Y + _wall * 3, _wall * 2, _brick);
            FillRectangle(spriteBatch, pixelTexture, offset.X + _wall * 3, offset.Y + _wall * 3, _wall * 2, _brick);

            // esquerda
            FillRectangle(spriteBatch, pixelTexture, offset.X + 0, offset.Y + 0, _brick, _wall);
            FillRectangle(spriteBatch, pixelTexture, offset.X + 0, offset.Y + _wall * 2, _brick, _wall);

            // direita
            FillRectangle(spriteBatch, pixelTexture, offset.X + _wall * 5, offset.Y + 0, _brick, _wall);
            FillRectangle(spriteBatch, pixelTexture, offset.X + _wall * 5, offset.Y + _wall * 2, _brick, _wall);
        }

        // preenche as paredes geradas
        for (var i = 0; i < 8; i++)
        {
            var p1 = _pillarPoints[i];
            switch (directions[i])
            {
                case PillarDirection.North:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X, offset.Y + p1.Y - _wall, _brick, _wall);
                    break;

                case PillarDirection.South:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X, offset.Y + p1.Y, _brick, _wall);
                    break;

                case PillarDirection.East:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X, offset.Y + p1.Y, _wall, _brick);
                    break;

                case PillarDirection.West:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X - _wall, offset.Y + p1.Y, _wall, _brick);
                    break;

                default:
                    throw new InvalidProgramException();
            }
        }

        // preenche possíveis vazios que podem ficar
        {
            for (var i = 0; i < 8; i++)
            {
                FillRectangle(spriteBatch, pixelTexture, offset.X + _pillarPoints[i].X, offset.Y + _pillarPoints[i].Y, _brick, _brick);
            }

            FillRectangle(spriteBatch, pixelTexture, offset.X + _wall * 5, offset.Y + _wall, _brick, _brick);
            FillRectangle(spriteBatch, pixelTexture, offset.X + _wall * 5, offset.Y + _wall * 3, _brick, _brick);
        }
    }

    private void FillRectangle(SpriteBatch spriteBatch, Texture2D pixelTexture, int x, int y, int width, int height)
    {
        spriteBatch.Draw(pixelTexture, new Rectangle(x, y, width, height), _wallColor);
    }
}