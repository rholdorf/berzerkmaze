using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace BerzerkMaze;

internal class MazeGame : Game
{
    private const int LOGICAL_WIDTH = MazeLayout.LogicalWidth;
    private const int LOGICAL_HEIGHT = MazeLayout.LogicalHeight;
    private const int MIN_DISPLAY_SCALE = 1;
    private const int MAX_DISPLAY_SCALE = 6;
    private GraphicsDeviceManager _graphics;
    private GameEngine _engine;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixelTexture;
    private RenderTarget2D _sceneTarget;
    private KeyboardState _previousKeyboardState;
    private int _displayScale = 3;

    public MazeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = LOGICAL_WIDTH * _displayScale;
        _graphics.PreferredBackBufferHeight = LOGICAL_HEIGHT * _displayScale;
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

        if (IsCommandPressed(keyboardState) && IsAnyNewKeyPress(keyboardState, Keys.OemPlus, Keys.Add))
        {
            SetDisplayScale(_displayScale + 1);
        }

        if (IsCommandPressed(keyboardState) && IsAnyNewKeyPress(keyboardState, Keys.OemMinus, Keys.Subtract))
        {
            SetDisplayScale(_displayScale - 1);
        }

        _engine.SetPlayerInput(
            keyboardState.IsKeyDown(Keys.W),
            keyboardState.IsKeyDown(Keys.S),
            keyboardState.IsKeyDown(Keys.A),
            keyboardState.IsKeyDown(Keys.D),
            keyboardState.IsKeyDown(Keys.Space));

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
            new Rectangle(0, 0, LOGICAL_WIDTH * _displayScale, LOGICAL_HEIGHT * _displayScale),
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

    private bool IsAnyNewKeyPress(KeyboardState current, params Keys[] keys)
    {
        for (var i = 0; i < keys.Length; i++)
        {
            if (IsNewKeyPress(keys[i], current))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCommandPressed(KeyboardState state)
    {
        return state.IsKeyDown(Keys.LeftWindows) || state.IsKeyDown(Keys.RightWindows);
    }

    private void SetDisplayScale(int newScale)
    {
        if (newScale < MIN_DISPLAY_SCALE)
        {
            newScale = MIN_DISPLAY_SCALE;
        }
        else if (newScale > MAX_DISPLAY_SCALE)
        {
            newScale = MAX_DISPLAY_SCALE;
        }

        if (newScale == _displayScale)
        {
            return;
        }

        _displayScale = newScale;
        _graphics.PreferredBackBufferWidth = LOGICAL_WIDTH * _displayScale;
        _graphics.PreferredBackBufferHeight = LOGICAL_HEIGHT * _displayScale;
        _graphics.ApplyChanges();
    }
}
