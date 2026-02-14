using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BerzerkMaze;

internal class GameEngine
{
    private enum PlayerEntrySide
    {
        North = 0,
        South = 1,
        West = 2,
        East = 3
    }

    private const int MaxRobots = 8;
    private const int RobotInitialY = 75;
    private const int RobotSourceYMax = 75;
    private const byte RobotRandomMax = 135;

    private static readonly byte[] RobotMotionDelayTable =
    [
        0x20, 0x28, 0x30, 0x38, 0x40, 0x48, 0x50, 0x58
    ];

    private static readonly byte[] RobotMissileDelayTable =
    [
        0x38, 0x40, 0x50, 0x60, 0x78, 0x90, 0xB0, 0xD0
    ];
    private static readonly int[] RobotIdleAnimationTable = [1, 2, 3, 4, 5, 6, 7, 8, 0];

    private const int RobotPixelScale = 1;
    private const int RobotWidth = 8 * RobotPixelScale;
    private const int RobotHeight = 9 * RobotPixelScale;
    private const int RobotDeathFrames = 4;
    private const int RobotVerticalGapUp = (9 + 1) * RobotPixelScale;
    private const int RobotVerticalGapDown = (9 + 2) * RobotPixelScale;

    private const int PlayerWidth = 8;
    private const int PlayerHeight = 12;
    private const byte PlayerFractionalDelay = 0x70; // move 7 out of 16 frames (NTSC)
    private const int PlayerRespawnDelayFrames = 40;
    private const int PlayerMissileSize = 2;

    private const int RobotMissileSize = 2;
    private const int RobotMissileAlignX = 8 * RobotPixelScale;
    private const int RobotMissileAlignY = 6 * RobotPixelScale;
    private const int RobotMissileStepX = 2 * RobotPixelScale;
    private const int RobotMissileStepY = 1 * RobotPixelScale;
    private static readonly sbyte[] VerticalPixelOffsets = [0, -1, 1, 0, 0, -1, 1, 0, 0, -1, 1, 0, 0, 0, 0, 0];
    private static readonly sbyte[] HorizontalPixelOffsets = [0, 0, 0, 0, -1, -1, -1, -1, 1, 1, 1, 1, 0, 0, 0, 0];
    private static readonly byte[] InitMissileXOffsetTable = [0, 7, 8, 0, 0, 0, 0, 0, 6, 6, 6];
    private static readonly byte[] InitMissileYOffsetTable = [0, 2, 7, 0, 5, 5, 7, 0, 5, 5, 7];

    private int _roomNumber;
    private int _frameCounter;
    private int _gameLevel = 1;
    private PillarDirection[] _directions;

    private readonly Renderer _renderer = new();
    private readonly List<RobotState> _robots = new(MaxRobots);
    private readonly List<Rectangle> _collisionWalls = new(24);
    private readonly PlayerState _player = new();
    private readonly PlayerMissileState _playerMissile = new();
    private readonly RobotMissileState _robotMissile = new();

    private byte _randomLo;
    private byte _randomHi;
    private byte _initRobotDelay;
    private byte _robotMotion;
    private byte _robotMotionDelay;
    private byte _playerMotion;
    private byte _playerInputMask;
    private bool _playerFirePressed;
    private PlayerEntrySide _nextEntrySide = PlayerEntrySide.West;

    public GameEngine()
    {
        _roomNumber = BerzerkMaze.Calculate(_roomNumber);
        LoadRoom();
    }

    public void Render(SpriteBatch spriteBatch, Texture2D pixelTexture, Point offset)
    {
        _renderer.Render(spriteBatch, pixelTexture, _directions, _robots, _player, _playerMissile, _robotMissile, offset);
    }

    public void Update()
    {
        _frameCounter++;

        UpdateDyingRobots();
        SortRobotsByVerticalPosition();
        UpdatePlayerState();
        UpdatePlayerMovement();
        UpdatePlayerMissile();
        CheckPlayerRobotCollision();

        if (_initRobotDelay != byte.MaxValue)
        {
            _initRobotDelay = (byte)((_initRobotDelay >> 1) | 0x80);
            UpdateRobotMissile();
            return;
        }

        _robotMotion = AddByteWithCarry(_robotMotion, _robotMotionDelay, out var shouldMoveRobots);
        if (shouldMoveRobots)
        {
            for (var i = 0; i < _robots.Count; i++)
            {
                if (_robots[i].IsDying)
                {
                    continue;
                }

                UpdateRobot(i);
            }
            UpdateIdleRobotAnimation();
        }

        UpdateRobotMissile();
    }

    public void SetPlayerInput(bool up, bool down, bool left, bool right, bool firePressed)
    {
        _playerInputMask = 0;
        if (up)
        {
            _playerInputMask |= 0x01;
        }

        if (down)
        {
            _playerInputMask |= 0x02;
        }

        if (left)
        {
            _playerInputMask |= 0x04;
        }

        if (right)
        {
            _playerInputMask |= 0x08;
        }

        _playerFirePressed = firePressed;
    }

    public void Up()
    {
        _roomNumber -= 32;
        _nextEntrySide = PlayerEntrySide.South;
        NextRoom();
    }

    public void Left()
    {
        _roomNumber--;
        _nextEntrySide = PlayerEntrySide.East;
        NextRoom();
    }

    public void Right()
    {
        _roomNumber++;
        _nextEntrySide = PlayerEntrySide.West;
        NextRoom();
    }

    public void Down()
    {
        _roomNumber += 32;
        _nextEntrySide = PlayerEntrySide.North;
        NextRoom();
    }

    private void NextRoom()
    {
        _gameLevel++;
        LoadRoom();
    }

    private void LoadRoom()
    {
        _directions = BerzerkMaze.GenerateRoom(_roomNumber);
        BuildCollisionWalls();
        ResetPlayerForRoom();
        SpawnRobotsForRoom();
        DeactivatePlayerMissile();
        DeactivateRobotMissile();
    }

    private void SpawnRobotsForRoom()
    {
        _robots.Clear();
        SeedRandomForRoom(_roomNumber);
        _initRobotDelay = 0;
        _robotMotion = 0;
        _robotMotionDelay = RobotMotionDelayTable[0];

        var y = RobotInitialY;
        for (var i = 0; i < MaxRobots; i++)
        {
            var animationIndex = NextRandom() & 0x07;
            var mappedY = MapSpawnY(y);
            var mappedX = FindSpawnX(mappedY);
            _robots.Add(new RobotState
            {
                X = mappedX,
                Y = mappedY,
                IdleAnimIndex = animationIndex,
                Direction = (RobotDirection)(NextRandom() & 0x03),
                WalkFrame = 0,
                IsMoving = false,
                DecisionCooldown = 3 + (NextRandom() & 0x07),
                IsDying = false,
                DeathFrame = 0
            });
            y -= 10;
        }
    }

    private void UpdateRobot(int robotIndex)
    {
        var robot = _robots[robotIndex];
        robot.IsMoving = false;

        robot.DecisionCooldown--;
        if (robot.DecisionCooldown <= 0)
        {
            robot.Direction = DetermineDirectionFromAsmStyle(robotIndex, robot);
            robot.DecisionCooldown = 3 + (NextRandom() & 0x07);
        }

        var nextX = robot.X;
        var nextY = robot.Y;
        switch (robot.Direction)
        {
            case RobotDirection.Up:
                nextY--;
                break;
            case RobotDirection.Down:
                nextY++;
                break;
            case RobotDirection.Left:
                nextX--;
                break;
            default:
                nextX++;
                break;
        }

        if (!CanMoveVerticallyWithoutStacking(robotIndex, nextY))
        {
            robot.Direction = DetermineDirectionFromAsmStyle(robotIndex, robot);
            robot.DecisionCooldown = 2 + (NextRandom() & 0x03);
            return;
        }

        var nextRect = new Rectangle(nextX, nextY, RobotWidth, RobotHeight);
        if (CollidesWithWall(nextRect))
        {
            StartExplosion(robotIndex);
            return;
        }

        var collidedRobotIndex = FindCollidingRobot(nextRect, robotIndex);
        if (collidedRobotIndex >= 0)
        {
            StartExplosion(robotIndex);
            StartExplosion(collidedRobotIndex);
            return;
        }

        robot.X = nextX;
        robot.Y = nextY;
        robot.IsMoving = true;
        robot.WalkFrame ^= 1;
    }

    private RobotDirection DetermineDirectionFromAsmStyle(int robotIndex, RobotState robot)
    {
        var selectedRobot = DetermineRobotToMove();

        if (robotIndex == selectedRobot && (_randomHi & 0x80) != 0)
        {
            if (_player.Y > robot.Y)
            {
                return RobotDirection.Down;
            }

            if (_player.Y < robot.Y)
            {
                return RobotDirection.Up;
            }
        }

        if (_player.X > robot.X)
        {
            return RobotDirection.Right;
        }

        if (_player.X < robot.X)
        {
            return RobotDirection.Left;
        }

        return robot.Direction;
    }

    private int DetermineRobotToMove()
    {
        var y = 3;
        var threshold = 0;
        while (y >= 0)
        {
            threshold += 34;
            if (threshold >= _player.X)
            {
                break;
            }

            y--;
        }

        if (_player.Y < MazeLayout.PlayfieldTop + MazeLayout.PlayfieldHeight / 2 - RobotHeight + 1)
        {
            y += 4;
        }

        return (y ^ _randomHi) & 0x07;
    }

    private void UpdateRobotMissile()
    {
        if (!_robotMissile.Active)
        {
            TryLaunchRobotMissile();
            return;
        }

        if (ShouldMoveRobotMissileThisFrame())
        {
            MoveRobotMissile();
        }

        CheckRobotMissileCollisions();
    }

    private void TryLaunchRobotMissile()
    {
        if (_initRobotDelay != byte.MaxValue)
        {
            return;
        }

        if (_gameLevel <= 1 || _robots.Count == 0)
        {
            return;
        }

        var robotIndex = _frameCounter & 0x07;
        if (robotIndex >= _robots.Count)
        {
            return;
        }

        var robot = _robots[robotIndex];
        if (robot.IsDying)
        {
            return;
        }

        var robotCenterX = robot.X + RobotWidth / 2;
        var robotCenterY = robot.Y + RobotHeight / 2;
        var playerCenterX = _player.X + PlayerWidth / 2;
        var playerCenterY = _player.Y + PlayerHeight / 2;

        RobotMissileDirection direction;
        if (playerCenterX >= robotCenterX - RobotMissileAlignX && playerCenterX <= robotCenterX + RobotMissileAlignX)
        {
            direction = playerCenterY >= robotCenterY ? RobotMissileDirection.Down : RobotMissileDirection.Up;
        }
        else if (playerCenterY >= robotCenterY - RobotMissileAlignY && playerCenterY <= robotCenterY + RobotMissileAlignY)
        {
            direction = robotCenterX >= playerCenterX ? RobotMissileDirection.Left : RobotMissileDirection.Right;
        }
        else
        {
            return;
        }

        LaunchRobotMissile(robot, direction);
    }

    private void LaunchRobotMissile(RobotState robot, RobotMissileDirection direction)
    {
        _robotMissile.Active = true;
        _robotMissile.Direction = direction;
        _robotMissile.DelayAccumulator = 0xFF;
        _robotMissile.FlightTicks = 0;

        var offsetX = direction switch
        {
            RobotMissileDirection.Right => 9,
            RobotMissileDirection.Left => 3,
            RobotMissileDirection.Down => 4,
            RobotMissileDirection.Up => 4,
            _ => 3
        };

        var offsetY = direction switch
        {
            RobotMissileDirection.Right => 7,
            RobotMissileDirection.Left => 7,
            RobotMissileDirection.Down => 6,
            RobotMissileDirection.Up => 1,
            _ => 0
        };

        _robotMissile.X = robot.X + offsetX;
        _robotMissile.Y = robot.Y + offsetY;
    }

    private bool ShouldMoveRobotMissileThisFrame()
    {
        if (_gameLevel >= 16)
        {
            return true;
        }

        var index = _gameLevel >> 1;
        if (index < 0)
        {
            index = 0;
        }
        else if (index >= RobotMissileDelayTable.Length)
        {
            index = RobotMissileDelayTable.Length - 1;
        }

        var add = RobotMissileDelayTable[index];
        if ((_gameLevel & 1) != 0)
        {
            add++;
        }

        _robotMissile.DelayAccumulator = AddByteWithCarry(_robotMissile.DelayAccumulator, add, out var carry);
        return carry;
    }

    private void MoveRobotMissile()
    {
        switch (_robotMissile.Direction)
        {
            case RobotMissileDirection.Down:
                _robotMissile.Y += RobotMissileStepY;
                break;
            case RobotMissileDirection.Up:
                _robotMissile.Y -= RobotMissileStepY;
                break;
            case RobotMissileDirection.Right:
                _robotMissile.X += RobotMissileStepX;
                break;
            case RobotMissileDirection.Left:
                _robotMissile.X -= RobotMissileStepX;
                break;
        }

        _robotMissile.FlightTicks++;
    }

    private void CheckRobotMissileCollisions()
    {
        if (!_robotMissile.Active)
        {
            return;
        }

        var missileRect = new Rectangle(_robotMissile.X, _robotMissile.Y, RobotMissileSize, RobotMissileSize);

        if (_robotMissile.X <= 0 || _robotMissile.X >= MazeLayout.LogicalWidth - RobotMissileSize ||
            _robotMissile.Y <= 0 || _robotMissile.Y >= MazeLayout.LogicalHeight - RobotMissileSize)
        {
            DeactivateRobotMissile();
            return;
        }

        if (CollidesWithWall(missileRect))
        {
            DeactivateRobotMissile();
            return;
        }

        for (var i = 0; i < _robots.Count; i++)
        {
            if (_robots[i].IsDying)
            {
                continue;
            }

            if (missileRect.Intersects(GetRobotBounds(_robots[i])))
            {
                DeactivateRobotMissile();
                return;
            }
        }

        var playerRect = new Rectangle(_player.X, _player.Y, PlayerWidth, PlayerHeight);
        if (missileRect.Intersects(playerRect))
        {
            KillPlayer();
            DeactivateRobotMissile();
        }
    }

    private void DeactivateRobotMissile()
    {
        _robotMissile.Active = false;
        _robotMissile.Direction = RobotMissileDirection.None;
        _robotMissile.DelayAccumulator = 0;
        _robotMissile.FlightTicks = 0;
    }

    private void ResetPlayerForRoom()
    {
        var centerX = MazeLayout.RoomOriginX + MazeLayout.RoomWidth / 2 - PlayerWidth / 2;
        var centerY = MazeLayout.RoomOriginY + MazeLayout.RoomHeight / 2 - (PlayerHeight + 2);
        var leftX = MazeLayout.RoomOriginX + 6;
        var rightX = MazeLayout.RoomOriginX + MazeLayout.RoomWidth - (PlayerWidth + 7);
        var topY = MazeLayout.RoomOriginY + 8;
        var bottomY = MazeLayout.RoomOriginY + MazeLayout.RoomHeight - (PlayerHeight + 8);

        switch (_nextEntrySide)
        {
            case PlayerEntrySide.North:
                _player.X = centerX;
                _player.Y = topY;
                _player.FacingMask = 0x02;
                break;
            case PlayerEntrySide.South:
                _player.X = centerX;
                _player.Y = bottomY;
                _player.FacingMask = 0x01;
                break;
            case PlayerEntrySide.East:
                _player.X = rightX;
                _player.Y = centerY;
                _player.FacingMask = 0x04;
                break;
            default:
                _player.X = leftX;
                _player.Y = centerY;
                _player.FacingMask = 0x08;
                break;
        }

        _player.IsDying = false;
        _player.RespawnTimer = 0;
        _player.IsMoving = false;
        _playerMotion = 0;

        for (var attempt = 0; attempt < 32; attempt++)
        {
            if (CanPlayerOccupy(_player.X, _player.Y))
            {
                return;
            }

            _player.Y += 1;
        }
    }

    private bool CanPlayerOccupy(int x, int y)
    {
        if (x < 0 || y < 0 || x + PlayerWidth > MazeLayout.LogicalWidth || y + PlayerHeight > MazeLayout.LogicalHeight)
        {
            return false;
        }

        var playerRect = new Rectangle(x, y, PlayerWidth, PlayerHeight);
        return !CollidesWithWall(playerRect);
    }

    private void UpdatePlayerMovement()
    {
        _player.IsMoving = false;
        if (_player.IsDying)
        {
            return;
        }

        if (_playerFirePressed)
        {
            TryLaunchPlayerMissile();
            _playerMotion = 0;
            return;
        }

        if (_playerInputMask == 0)
        {
            _playerMotion = 0;
            return;
        }

        _playerMotion = AddByteWithCarry(_playerMotion, PlayerFractionalDelay, out var shouldMove);
        if (!shouldMove)
        {
            return;
        }

        var dx = HorizontalPixelOffsets[_playerInputMask];
        var dy = VerticalPixelOffsets[_playerInputMask];
        if (dx == 0 && dy == 0)
        {
            return;
        }

        var nextX = _player.X + dx;
        var nextY = _player.Y + dy;
        if (!CanPlayerOccupy(nextX, nextY))
        {
            return;
        }

        _player.X = nextX;
        _player.Y = nextY;
        _player.IsMoving = true;
        _player.WalkFrame ^= 1;
        _player.FacingMask = _playerInputMask;
    }

    private void TryLaunchPlayerMissile()
    {
        if (_playerMissile.Active)
        {
            return;
        }

        _playerMissile.Active = true;
        _playerMissile.DirectionMask = _player.FacingMask;
        _playerMissile.DelayAccumulator = 0;
        var dir = _playerMissile.DirectionMask;
        if (dir >= InitMissileXOffsetTable.Length)
        {
            dir = 0x08;
        }

        _playerMissile.X = _player.X + InitMissileXOffsetTable[dir];
        _playerMissile.Y = _player.Y + InitMissileYOffsetTable[dir];
    }

    private void SeedRandomForRoom(int roomNumber)
    {
        var value = (uint)roomNumber;
        _randomHi = (byte)((value >> 8) & 0xFF);
        _randomLo = (byte)(value & 0xFF);
        _randomLo ^= 0xB5;
    }

    private byte NextRandom()
    {
        byte a = _randomLo;

        _ = ShiftLeft(ref a);
        a ^= _randomLo;
        _ = ShiftLeft(ref a);
        var carry = ShiftLeft(ref a);

        carry = RotateLeft(ref _randomHi, carry);
        _ = RotateLeft(ref _randomLo, carry);

        a = (byte)(_randomHi & 0x7F);
        byte temp01 = a;

        while (true)
        {
            if (a <= RobotRandomMax)
            {
                return a;
            }

            temp01 >>= 1;
            a = temp01;
        }
    }

    private static bool ShiftLeft(ref byte value)
    {
        var carry = (value & 0x80) != 0;
        value = (byte)(value << 1);
        return carry;
    }

    private static bool RotateLeft(ref byte value, bool carryIn)
    {
        var carryOut = (value & 0x80) != 0;
        value = (byte)((value << 1) | (carryIn ? 1 : 0));
        return carryOut;
    }

    private static byte AddByteWithCarry(byte a, byte b, out bool carry)
    {
        var sum = a + b;
        carry = sum > 0xFF;
        return (byte)sum;
    }

    private void BuildCollisionWalls()
    {
        _collisionWalls.Clear();

        var ox = MazeLayout.RoomOriginX;
        var oy = MazeLayout.RoomOriginY;
        var w = MazeLayout.Wall;
        var b = MazeLayout.Brick;

        _collisionWalls.Add(new Rectangle(ox + 0, oy + 0, w * 2, b));
        _collisionWalls.Add(new Rectangle(ox + w * 3, oy + 0, w * 2, b));
        _collisionWalls.Add(new Rectangle(ox + 0, oy + w * 3, w * 2, b));
        _collisionWalls.Add(new Rectangle(ox + w * 3, oy + w * 3, w * 2, b));

        _collisionWalls.Add(new Rectangle(ox + 0, oy + 0, b, w));
        _collisionWalls.Add(new Rectangle(ox + 0, oy + w * 2, b, w));
        _collisionWalls.Add(new Rectangle(ox + w * 5, oy + 0, b, w));
        _collisionWalls.Add(new Rectangle(ox + w * 5, oy + w * 2, b, w));

        for (var i = 0; i < _directions.Length; i++)
        {
            var p = MazeLayout.PillarPoints[i];
            switch (_directions[i])
            {
                case PillarDirection.North:
                    _collisionWalls.Add(new Rectangle(p.X, p.Y - w, b, w));
                    break;
                case PillarDirection.South:
                    _collisionWalls.Add(new Rectangle(p.X, p.Y, b, w));
                    break;
                case PillarDirection.East:
                    _collisionWalls.Add(new Rectangle(p.X, p.Y, w, b));
                    break;
                case PillarDirection.West:
                    _collisionWalls.Add(new Rectangle(p.X - w, p.Y, w, b));
                    break;
            }
        }

        for (var i = 0; i < MazeLayout.PillarPoints.Length; i++)
        {
            var p = MazeLayout.PillarPoints[i];
            _collisionWalls.Add(new Rectangle(p.X, p.Y, b, b));
        }

        _collisionWalls.Add(new Rectangle(ox + w * 5, oy + w, b, b));
        _collisionWalls.Add(new Rectangle(ox + w * 5, oy + w * 3, b, b));
    }

    private bool CanOccupy(int x, int y)
    {
        if (x < 0 || y < 0 || x + RobotWidth > MazeLayout.LogicalWidth || y + RobotHeight > MazeLayout.LogicalHeight)
        {
            return false;
        }

        var robotRect = new Rectangle(x, y, RobotWidth, RobotHeight);
        return !CollidesWithWall(robotRect) && FindCollidingRobot(robotRect, -1) < 0;
    }

    private int FindSpawnX(int mappedY)
    {
        var fallback = MapSpawnX(0);
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var sourceX = NextRandom();
            var mappedX = MapSpawnX(sourceX);
            if (CanOccupy(mappedX, mappedY))
            {
                return mappedX;
            }
        }

        return fallback;
    }

    private static Rectangle GetRobotBounds(RobotState robot)
    {
        return new Rectangle(robot.X, robot.Y, RobotWidth, RobotHeight);
    }

    private bool CollidesWithWall(Rectangle rect)
    {
        for (var i = 0; i < _collisionWalls.Count; i++)
        {
            if (rect.Intersects(_collisionWalls[i]))
            {
                return true;
            }
        }

        return false;
    }

    private int FindCollidingRobot(Rectangle robotRect, int selfIndex)
    {
        for (var i = 0; i < _robots.Count; i++)
        {
            if (i == selfIndex || _robots[i].IsDying)
            {
                continue;
            }

            if (robotRect.Intersects(GetRobotBounds(_robots[i])))
            {
                return i;
            }
        }

        return -1;
    }

    private void StartExplosion(int index)
    {
        if (index < 0 || index >= _robots.Count)
        {
            return;
        }

        var robot = _robots[index];
        if (robot.IsDying)
        {
            return;
        }

        robot.IsDying = true;
        robot.DeathFrame = 0;
        robot.IsMoving = false;
    }

    private void UpdateDyingRobots()
    {
        var removedAny = false;
        for (var i = _robots.Count - 1; i >= 0; i--)
        {
            var robot = _robots[i];
            if (!robot.IsDying)
            {
                continue;
            }

            robot.DeathFrame++;
            if (robot.DeathFrame >= RobotDeathFrames)
            {
                _robots.RemoveAt(i);
                removedAny = true;
            }
        }

        if (removedAny)
        {
            IncreaseRobotSpeedAfterRobotRemoval();
        }
    }

    private bool CanMoveVerticallyWithoutStacking(int robotIndex, int nextY)
    {
        var robot = _robots[robotIndex];

        if (robot.Direction == RobotDirection.Up)
        {
            if (robotIndex == 0)
            {
                return true;
            }

            var previous = _robots[robotIndex - 1];
            if (previous.IsDying)
            {
                return true;
            }

            return previous.Y + RobotVerticalGapUp < nextY;
        }

        if (robot.Direction == RobotDirection.Down)
        {
            if (robotIndex >= _robots.Count - 1)
            {
                return true;
            }

            var next = _robots[robotIndex + 1];
            if (next.IsDying)
            {
                return true;
            }

            return nextY + RobotVerticalGapDown < next.Y;
        }

        return true;
    }

    private void SortRobotsByVerticalPosition()
    {
        _robots.Sort(static (a, b) => a.Y.CompareTo(b.Y));
    }

    private void IncreaseRobotSpeedAfterRobotRemoval()
    {
        _robotMotionDelay = unchecked((byte)(_robotMotionDelay + 2));
    }

    private static int MapSpawnX(int sourceX)
    {
        var innerLeft = MazeLayout.RoomOriginX + MazeLayout.Brick;
        var innerWidth = MazeLayout.Wall * 5 - MazeLayout.Brick;
        return innerLeft + (sourceX * (innerWidth - RobotWidth)) / 134;
    }

    private static int MapSpawnY(int sourceY)
    {
        var innerTop = MazeLayout.RoomOriginY + MazeLayout.Brick;
        var innerHeight = MazeLayout.Wall * 3 - MazeLayout.Brick;
        return innerTop + (sourceY * (innerHeight - RobotHeight)) / RobotSourceYMax;
    }

    private void UpdateIdleRobotAnimation()
    {
        for (var i = 0; i < _robots.Count; i++)
        {
            var robot = _robots[i];
            if (robot.IsDying || robot.IsMoving)
            {
                continue;
            }

            var index = robot.IdleAnimIndex;
            if (index < 0 || index >= RobotIdleAnimationTable.Length)
            {
                index = 0;
            }

            robot.IdleAnimIndex = RobotIdleAnimationTable[index];
        }
    }

    private void UpdatePlayerMissile()
    {
        if (!_playerMissile.Active)
        {
            return;
        }

        var dir = _playerMissile.DirectionMask;
        if (dir >= VerticalPixelOffsets.Length)
        {
            DeactivatePlayerMissile();
            return;
        }

        var vy = VerticalPixelOffsets[dir];
        var vx = HorizontalPixelOffsets[dir];
        _playerMissile.Y += vy;
        _playerMissile.X += vx * 2;

        CheckPlayerMissileCollisions();
    }

    private void CheckPlayerMissileCollisions()
    {
        if (!_playerMissile.Active)
        {
            return;
        }

        var missileRect = new Rectangle(_playerMissile.X, _playerMissile.Y, PlayerMissileSize, PlayerMissileSize);
        if (_playerMissile.X <= 0 || _playerMissile.X >= MazeLayout.LogicalWidth - PlayerMissileSize ||
            _playerMissile.Y <= 0 || _playerMissile.Y >= MazeLayout.LogicalHeight - PlayerMissileSize)
        {
            DeactivatePlayerMissile();
            return;
        }

        if (CollidesWithWall(missileRect))
        {
            DeactivatePlayerMissile();
            return;
        }

        for (var i = 0; i < _robots.Count; i++)
        {
            if (_robots[i].IsDying)
            {
                continue;
            }

            if (missileRect.Intersects(GetRobotBounds(_robots[i])))
            {
                StartExplosion(i);
                DeactivatePlayerMissile();
                return;
            }
        }

        if (_robotMissile.Active)
        {
            var robotMissileRect = new Rectangle(_robotMissile.X, _robotMissile.Y, RobotMissileSize, RobotMissileSize);
            if (missileRect.Intersects(robotMissileRect))
            {
                DeactivatePlayerMissile();
                DeactivateRobotMissile();
            }
        }
    }

    private void DeactivatePlayerMissile()
    {
        _playerMissile.Active = false;
        _playerMissile.DirectionMask = 0;
        _playerMissile.DelayAccumulator = 0;
    }

    private void CheckPlayerRobotCollision()
    {
        if (_player.IsDying)
        {
            return;
        }

        var playerRect = new Rectangle(_player.X, _player.Y, PlayerWidth, PlayerHeight);
        for (var i = 0; i < _robots.Count; i++)
        {
            if (_robots[i].IsDying)
            {
                continue;
            }

            if (playerRect.Intersects(GetRobotBounds(_robots[i])))
            {
                KillPlayer();
                return;
            }
        }
    }

    private void KillPlayer()
    {
        if (_player.IsDying)
        {
            return;
        }

        _player.IsDying = true;
        _player.IsMoving = false;
        _player.RespawnTimer = PlayerRespawnDelayFrames;
        DeactivatePlayerMissile();
    }

    private void UpdatePlayerState()
    {
        if (!_player.IsDying)
        {
            return;
        }

        _player.RespawnTimer--;
        if (_player.RespawnTimer <= 0)
        {
            ResetPlayerForRoom();
        }
    }
}
