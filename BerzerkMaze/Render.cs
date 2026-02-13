using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BerzerkMaze
{
    class Renderer
    {
        private readonly int _brick;
        private readonly int _bricksPerWall;
        private readonly int _wall;
        private readonly Point[] _pillarPoints;
        private readonly Brush _wallBrush = new SolidBrush(Color.FromArgb(11, 36, 251));

        public Renderer()
        {
            _brick = 4;
            _bricksPerWall = 12;
            _wall = _brick * _bricksPerWall;

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

        public void Render(Graphics g, PillarDirection[] directions)
        {
            // paredes fixas
            {
                // superior
                g.FillRectangle(_wallBrush, 0, 0, _wall * 2, _brick);
                g.FillRectangle(_wallBrush, _wall * 3, 0, _wall * 2, _brick);

                // inferior
                g.FillRectangle(_wallBrush, 0, _wall * 3, _wall * 2, _brick);
                g.FillRectangle(_wallBrush, _wall * 3, _wall * 3, _wall * 2, _brick);

                // esquerda
                g.FillRectangle(_wallBrush, 0, 0, _brick, _wall);
                g.FillRectangle(_wallBrush, 0, _wall * 2, _brick, _wall);

                // direita
                g.FillRectangle(_wallBrush, _wall * 5, 0, _brick, _wall);
                g.FillRectangle(_wallBrush, _wall * 5, _wall * 2, _brick, _wall);
            }

            // preenche as paredes geradas
            for (int i = 0; i < 8; i++)
            {
                Point p1 = _pillarPoints[i];
                switch (directions[i])
                {
                    case PillarDirection.North:
                        g.FillRectangle(_wallBrush, p1.X, p1.Y - _wall, _brick, _wall);
                        break;

                    case PillarDirection.South:
                        g.FillRectangle(_wallBrush, p1.X, p1.Y, _brick, _wall);
                        break;

                    case PillarDirection.East:
                        g.FillRectangle(_wallBrush, p1.X, p1.Y, _wall, _brick);
                        break;

                    case PillarDirection.West:
                        g.FillRectangle(_wallBrush, p1.X - _wall, p1.Y, _wall, _brick);
                        break;

                    default:
                        throw new InvalidProgramException();
                }
            }

            // preenche possíveis vazios que podem ficar
            {
                for (int i = 0; i < 8; i++)
                {
                    g.FillRectangle(_wallBrush, _pillarPoints[i].X, _pillarPoints[i].Y, _brick, _brick);
                }

                g.FillRectangle(_wallBrush, _wall * 5, _wall, _brick, _brick);
                g.FillRectangle(_wallBrush, _wall * 5, _wall * 3, _brick, _brick);
            }
        }
    }
}
