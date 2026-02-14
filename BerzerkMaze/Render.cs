using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BerzerkMaze;

internal class Renderer
{
    private readonly Color _wallColor = new(11, 36, 251);
    private readonly Color _robotColor = new(245, 245, 84);
    private readonly Color _playerColor = new(80, 255, 120);
    private readonly Color _playerMissileColor = new(120, 255, 255);
    private readonly Color _robotMissileColor = new(255, 80, 80);
    private const int ROBOT_PIXEL_SCALE = 1;
    private const int ROBOT_WIDTH = 8;
    private const int ROBOT_HEIGHT = 9;
    private static readonly byte[][] StandingAnimations =
    [
        [0x3C, 0x7E, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00], // StandingAnimation0
        [0x3C, 0x3E, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00], // StandingAnimation1
        [0x3C, 0x1E, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00], // StandingAnimation2
        [0x3C, 0x4E, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00], // StandingAnimation3
        [0x3C, 0x66, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00], // StandingAnimation4
        [0x3C, 0x72, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00], // StandingAnimation5
        [0x3C, 0x78, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00], // StandingAnimation6
        [0x3C, 0x7C, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x66, 0x00]  // StandingAnimation7
    ];

    public Renderer()
    {
    }

    private static readonly byte[][] WalkingLeftAnimations =
    [
        [0x3C, 0x3E, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x6C, 0x00], // WalkingLeftAnimation0
        [0x3C, 0x3E, 0xFF, 0xBD, 0xBD, 0x18, 0x18, 0x38, 0x00]  // WalkingLeftAnimation1
    ];
    private static readonly byte[][] WalkingRightAnimations =
    [
        [0x3C, 0x7C, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x36, 0x00], // WalkingRightAnimation0
        [0x3C, 0x7C, 0xFF, 0xBD, 0xBD, 0x18, 0x18, 0x1C, 0x00]  // WalkingRightAnimation1
    ];
    private static readonly byte[][] WalkingUpAnimations =
    [
        [0x3C, 0x7E, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x64, 0x06], // WalkingUpAnimation0
        [0x3C, 0x7E, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x26, 0x60]  // WalkingUpAnimation1
    ];
    private static readonly byte[][] WalkingDownAnimations =
    [
        [0x3C, 0x66, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x64, 0x06], // WalkingDownAnimation0
        [0x3C, 0x66, 0xFF, 0xBD, 0xBD, 0x24, 0x24, 0x26, 0x60]  // WalkingDownAnimation1
    ];
    private static readonly byte[][] DeathAnimations =
    [
        [0x00, 0x00, 0x00, 0x24, 0x18, 0x00, 0x00, 0x00, 0x00], // DeathAnimation0
        [0x14, 0x42, 0x81, 0x24, 0x00, 0x00, 0x52, 0x24, 0x00], // DeathAnimation1
        [0x42, 0x81, 0x24, 0x00, 0x42, 0x3C, 0x81, 0x42, 0x00]  // DeathAnimation2
    ];
    private static readonly int[] DeathAnimationFrameMap = [0, 1, 2, 2];
    private static readonly byte[] PlayerStationary =
    [
        0x00, 0x18, 0x18, 0x00, 0x3C, 0x5A, 0x5A, 0x18, 0x18, 0x18, 0x1C, 0x00
    ];
    private static readonly byte[] PlayerRunning0 =
    [
        0x00, 0x18, 0x18, 0x00, 0x38, 0x58, 0x3C, 0x18, 0x08, 0x64, 0x44, 0x00
    ];
    private static readonly byte[] PlayerRunning1 =
    [
        0x00, 0x18, 0x18, 0x00, 0x38, 0x18, 0x3C, 0x18, 0xF4, 0x82, 0x03, 0x00
    ];
    private static readonly byte[] PlayerDeath =
    [
        0x3C, 0x24, 0x24, 0x3C, 0xC3, 0xA5, 0xA5, 0xE7, 0x26, 0x22, 0x2E, 0x38
    ];

    public void Render(SpriteBatch spriteBatch, Texture2D pixelTexture, PillarDirection[] directions, IReadOnlyList<RobotState> robots, PlayerState player, PlayerMissileState playerMissile, RobotMissileState robotMissile, Point offset)
    {
        // paredes fixas
        {
            // superior
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + 0, offset.Y + MazeLayout.RoomOriginY + 0, MazeLayout.Wall * 2, MazeLayout.Brick);
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + MazeLayout.Wall * 3, offset.Y + MazeLayout.RoomOriginY + 0, MazeLayout.Wall * 2, MazeLayout.Brick);

            // inferior
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + 0, offset.Y + MazeLayout.RoomOriginY + MazeLayout.Wall * 3, MazeLayout.Wall * 2, MazeLayout.Brick);
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + MazeLayout.Wall * 3, offset.Y + MazeLayout.RoomOriginY + MazeLayout.Wall * 3, MazeLayout.Wall * 2, MazeLayout.Brick);

            // esquerda
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + 0, offset.Y + MazeLayout.RoomOriginY + 0, MazeLayout.Brick, MazeLayout.Wall);
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + 0, offset.Y + MazeLayout.RoomOriginY + MazeLayout.Wall * 2, MazeLayout.Brick, MazeLayout.Wall);

            // direita
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + MazeLayout.Wall * 5, offset.Y + MazeLayout.RoomOriginY + 0, MazeLayout.Brick, MazeLayout.Wall);
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + MazeLayout.Wall * 5, offset.Y + MazeLayout.RoomOriginY + MazeLayout.Wall * 2, MazeLayout.Brick, MazeLayout.Wall);
        }

        // preenche as paredes geradas
        for (var i = 0; i < 8; i++)
        {
            var p1 = MazeLayout.PillarPoints[i];
            switch (directions[i])
            {
                case PillarDirection.North:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X, offset.Y + p1.Y - MazeLayout.Wall, MazeLayout.Brick, MazeLayout.Wall);
                    break;

                case PillarDirection.South:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X, offset.Y + p1.Y, MazeLayout.Brick, MazeLayout.Wall);
                    break;

                case PillarDirection.East:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X, offset.Y + p1.Y, MazeLayout.Wall, MazeLayout.Brick);
                    break;

                case PillarDirection.West:
                    FillRectangle(spriteBatch, pixelTexture, offset.X + p1.X - MazeLayout.Wall, offset.Y + p1.Y, MazeLayout.Wall, MazeLayout.Brick);
                    break;

                default:
                    throw new InvalidProgramException();
            }
        }

        // preenche possíveis vazios que podem ficar
        {
            for (var i = 0; i < 8; i++)
            {
                FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.PillarPoints[i].X, offset.Y + MazeLayout.PillarPoints[i].Y, MazeLayout.Brick, MazeLayout.Brick);
            }

            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + MazeLayout.Wall * 5, offset.Y + MazeLayout.RoomOriginY + MazeLayout.Wall, MazeLayout.Brick, MazeLayout.Brick);
            FillRectangle(spriteBatch, pixelTexture, offset.X + MazeLayout.RoomOriginX + MazeLayout.Wall * 5, offset.Y + MazeLayout.RoomOriginY + MazeLayout.Wall * 3, MazeLayout.Brick, MazeLayout.Brick);
        }

        RenderRobots(spriteBatch, pixelTexture, robots, offset);
        RenderPlayer(spriteBatch, pixelTexture, player, offset);
        RenderPlayerMissile(spriteBatch, pixelTexture, playerMissile, offset);
        RenderRobotMissile(spriteBatch, pixelTexture, robotMissile, offset);
    }

    private void RenderRobots(SpriteBatch spriteBatch, Texture2D pixelTexture, IReadOnlyList<RobotState> robots, Point offset)
    {
        for (var i = 0; i < robots.Count; i++)
        {
            var robot = robots[i];
            var position = MapRobotPosition(robot, offset);
            var frame = ResolveRobotFrame(robot);
            DrawSprite(spriteBatch, pixelTexture, frame, position.X, position.Y);
        }
    }

    private static IReadOnlyList<byte> ResolveRobotFrame(RobotState robot)
    {
        if (robot.IsDying)
        {
            var index = robot.DeathFrame;
            if (index < 0)
            {
                index = 0;
            }
            else if (index >= DeathAnimationFrameMap.Length)
            {
                index = DeathAnimationFrameMap.Length - 1;
            }

            return DeathAnimations[DeathAnimationFrameMap[index]];
        }

        if (!robot.IsMoving)
        {
            var standingIndex = robot.IdleAnimIndex;
            if (standingIndex == 8)
            {
                standingIndex = 0;
            }

            return StandingAnimations[standingIndex % StandingAnimations.Length];
        }

        var walkFrame = robot.WalkFrame & 0x01;
        return robot.Direction switch
        {
            RobotDirection.Left => WalkingLeftAnimations[walkFrame],
            RobotDirection.Right => WalkingRightAnimations[walkFrame],
            RobotDirection.Up => WalkingUpAnimations[walkFrame],
            _ => WalkingDownAnimations[walkFrame]
        };
    }

    private Point MapRobotPosition(RobotState robot, Point offset)
    {
        return new Point(offset.X + robot.X, offset.Y + robot.Y);
    }

    private void DrawSprite(SpriteBatch spriteBatch, Texture2D pixelTexture, IReadOnlyList<byte> rows, int x, int y)
    {
        for (var row = 0; row < rows.Count; row++)
        {
            var bits = rows[row];
            for (var col = 0; col < 8; col++)
            {
                var mask = 1 << (7 - col);
                if ((bits & mask) == 0)
                {
                    continue;
                }

                FillRectangle(
                    spriteBatch,
                    pixelTexture,
                    x + col * ROBOT_PIXEL_SCALE,
                    y + row * ROBOT_PIXEL_SCALE,
                    ROBOT_PIXEL_SCALE,
                    ROBOT_PIXEL_SCALE,
                    _robotColor);
            }
        }
    }

    private void RenderPlayer(SpriteBatch spriteBatch, Texture2D pixelTexture, PlayerState player, Point offset)
    {
        var frame = player.IsDying
            ? PlayerDeath
            : player.IsMoving
            ? (player.WalkFrame & 0x01) == 0 ? PlayerRunning0 : PlayerRunning1
            : PlayerStationary;
        DrawSprite(spriteBatch, pixelTexture, frame, offset.X + player.X, offset.Y + player.Y, _playerColor);
    }

    private void RenderRobotMissile(SpriteBatch spriteBatch, Texture2D pixelTexture, RobotMissileState robotMissile, Point offset)
    {
        if (!robotMissile.Active)
        {
            return;
        }

        FillRectangle(spriteBatch, pixelTexture, offset.X + robotMissile.X, offset.Y + robotMissile.Y, 2, 2, _robotMissileColor);
    }

    private void RenderPlayerMissile(SpriteBatch spriteBatch, Texture2D pixelTexture, PlayerMissileState playerMissile, Point offset)
    {
        if (!playerMissile.Active)
        {
            return;
        }

        FillRectangle(spriteBatch, pixelTexture, offset.X + playerMissile.X, offset.Y + playerMissile.Y, 2, 2, _playerMissileColor);
    }

    private void FillRectangle(SpriteBatch spriteBatch, Texture2D pixelTexture, int x, int y, int width, int height)
    {
        FillRectangle(spriteBatch, pixelTexture, x, y, width, height, _wallColor);
    }

    private static void FillRectangle(SpriteBatch spriteBatch, Texture2D pixelTexture, int x, int y, int width, int height, Color color)
    {
        spriteBatch.Draw(pixelTexture, new Rectangle(x, y, width, height), color);
    }

    private void DrawSprite(SpriteBatch spriteBatch, Texture2D pixelTexture, IReadOnlyList<byte> rows, int x, int y, Color color)
    {
        for (var row = 0; row < rows.Count; row++)
        {
            var bits = rows[row];
            for (var col = 0; col < 8; col++)
            {
                var mask = 1 << (7 - col);
                if ((bits & mask) == 0)
                {
                    continue;
                }

                FillRectangle(spriteBatch, pixelTexture, x + col, y + row, 1, 1, color);
            }
        }
    }
}
