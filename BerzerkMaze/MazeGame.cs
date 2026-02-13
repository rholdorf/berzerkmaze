using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BerzerkMaze
{
    class MazeGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private GameEngine _engine;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixelTexture;
        private KeyboardState _previousKeyboardState;

        public MazeGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 265;
            _graphics.PreferredBackBufferHeight = 169;
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
            _pixelTexture.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

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

            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _engine.Render(_spriteBatch, _pixelTexture, new Point(10, 10));
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

            if (_spriteBatch != null)
            {
                _spriteBatch.Dispose();
                _spriteBatch = null;
            }

            base.UnloadContent();
        }

        private bool IsNewKeyPress(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }
    }
}
