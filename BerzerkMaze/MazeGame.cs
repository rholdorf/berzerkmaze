using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace BerzerkMaze;

internal class MazeGame : Game
{
    private const int LOGICAL_WIDTH = MazeLayout.LogicalWidth;
    private const int LOGICAL_HEIGHT = MazeLayout.LogicalHeight;
    private const int DISPLAY_SCALE = 2;
    private GraphicsDeviceManager _graphics;
    private GameEngine _engine;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixelTexture;
    private RenderTarget2D _sceneTarget;
    private KeyboardState _previousKeyboardState;

    public MazeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = LOGICAL_WIDTH * DISPLAY_SCALE;
        _graphics.PreferredBackBufferHeight = LOGICAL_HEIGHT * DISPLAY_SCALE;
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        IsMouseVisible = true;
        Window.Title = "Berzerk Maze";
    }

    protected override void Initialize()
    {
        _engine = new GameEngine();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);
        _sceneTarget = new RenderTarget2D(GraphicsDevice, LOGICAL_WIDTH, LOGICAL_HEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        _engine.BeginInputFrame();
            
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (IsNewKeyPress(Keys.Up, keyboardState))
        {
            _engine.Up();
        }

        if (IsNewKeyPress(Keys.Down, keyboardState))
        {
            _engine.Down();
        }

        if (IsNewKeyPress(Keys.Left, keyboardState))
        {
            _engine.Left();
        }

        if (IsNewKeyPress(Keys.Right, keyboardState))
        {
            _engine.Right();
        }

        if (keyboardState.IsKeyDown(Keys.W))
        {
            _engine.MovePlayerUp();
        }

        if (keyboardState.IsKeyDown(Keys.S))
        {
            _engine.MovePlayerDown();
        }

        if (keyboardState.IsKeyDown(Keys.A))
        {
            _engine.MovePlayerLeft();
        }

        if (keyboardState.IsKeyDown(Keys.D))
        {
            _engine.MovePlayerRight();
        }

        _engine.Update();
        _previousKeyboardState = keyboardState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_sceneTarget);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _engine.Render(_spriteBatch, _pixelTexture, Point.Zero);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(
            _sceneTarget,
            new Rectangle(0, 0, LOGICAL_WIDTH * DISPLAY_SCALE, LOGICAL_HEIGHT * DISPLAY_SCALE),
            Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        if (_pixelTexture != null)
        {
            _pixelTexture.Dispose();
            _pixelTexture = null;
        }

        if (_sceneTarget != null)
        {
            _sceneTarget.Dispose();
            _sceneTarget = null;
        }

        if (_spriteBatch != null)
        {
            _spriteBatch.Dispose();
            _spriteBatch = null;
        }

        if (_graphics != null)
        {
            _graphics.Dispose();
            _graphics = null;
        }

        base.UnloadContent();
    }

    private bool IsNewKeyPress(Keys key, KeyboardState current)
    {
        return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }
}
