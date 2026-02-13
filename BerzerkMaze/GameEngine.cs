using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerzerkMaze
{
    class GameEngine
    {
        // http://www.robotron2084guidebook.com/home/games/berzerk/mazegenerator/map/

        private int _roomNumber = 0;
        private Graphics _g;
        private Graphics _dbg;
        private PillarDirection[] _directions;
        private Bitmap _doubleBuffer;
        private Renderer _renderer = new Renderer();

        public GameEngine(Graphics g)
        {
            // walls are 4 pixels thick
            // 12 bricks form a wall
            // 12*4 == 48 pixels
            // room is 5 walls by 3 walls
            // then: 5walls * 48 pixels each by 3 walls * 48 pixels each == 240x144 (+4 thick na horizontal e + 4 thick na vertical) = 244v148
            _doubleBuffer = new Bitmap(245, 149);
            _dbg = Graphics.FromImage(_doubleBuffer);
            _roomNumber = BerzerkMaze.Calculate(_roomNumber);
            _g = g;
            UpdateDirections();
        }

        private void UpdateDirections()
        {
            _directions = BerzerkMaze.GenerateRoom(_roomNumber);
        }

        public void Update()
        {
            _dbg.Clear(Color.Black);
            _renderer.Render(_dbg, _directions);
            _g.Clear(Color.Black);
            _g.DrawImage(_doubleBuffer, 10, 10);
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
}
