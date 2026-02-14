using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace BerzerkMaze
{
    public partial class Form1 : Form
    {

        /*
         * Como gerar o labirinto
         * http://www.robotron2084guidebook.com/home/games/berzerk/mazegenerator/code/
         * 
         * Explicações sobre o mapa
         * http://www.robotron2084guidebook.com/home/games/berzerk/mazegenerator/map/
         * 
         * Sequencia de paredes
         * http://www.robotron2084guidebook.com/wp-content/uploads/2013/03/SR-frenzyrooms.txt
         * 
         * Explicações adicionais ao labirinto:
         * http://www.robotron2084guidebook.com/home/games/berzerk/mazegenerator/
         * 
         */

        private GameEngine _engine;


        public Form1()
        {
            InitializeComponent();
            _engine = new GameEngine(this.CreateGraphics());
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Up:
                    _engine.Up();
                    break;

                case Keys.Down:
                    _engine.Down();
                    break;

                case Keys.Left:
                    _engine.Left();
                    break;

                case Keys.Right:
                    _engine.Right();
                    break;
            }
            _engine.Update();
        }



    }
}
