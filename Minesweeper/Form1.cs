using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace Minesweeper
{
    public partial class Form1 : Form
    {
        public const int WIDTH = 20;
        public const int HEIGHT = 20;

        public const int MINES = 20;

        public const int TILE_SIZE = 16;

        public const int HMARGIN = 30;
        public const int VMARGIN = 50;

        public const int PROXIMITY_MAX = 9;

        private PictureBox[,] grid;

        public Form1()
        {            
            InitializeComponent();
            this.Size = new Size(WIDTH * TILE_SIZE + HMARGIN, 
                                 HEIGHT * TILE_SIZE + VMARGIN);
            this.CenterToScreen();
            this.grid = this.CreateGrid(WIDTH, HEIGHT);
            this.grid = this.SetMines(WIDTH, HEIGHT, MINES, this.grid);
            //CountMinesBoard(WIDTH, HEIGHT, this.grid);
        }

        private void Tile_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (pb.Tag.Equals(-1))
            {
                int xindex = pb.Location.X / 16;
                int yindex = pb.Location.Y / 16;

                CountMinesAroundCell(xindex, yindex, this.grid);
            }            
        }

        //TODO: Figure out the different clicks and subscribe the grid
        private void Tile_RightClick(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (pb.Tag.Equals(-1))
            {
                pb.BackgroundImage = Minesweeper.Properties.Resources.flag;
                pb.Tag = -3;
            }
        }

        private Image MineTile(int i)
        {
            switch (i)
            {
                case 0:
                    return Minesweeper.Properties.Resources.num0;
                case 1:
                    return Minesweeper.Properties.Resources.num1;
                case 2:
                    return Minesweeper.Properties.Resources.num2;
                case 3:
                    return Minesweeper.Properties.Resources.num3;
                case 4:
                    return Minesweeper.Properties.Resources.num4;
                case 5:
                    return Minesweeper.Properties.Resources.num5;
                case 6:
                    return Minesweeper.Properties.Resources.num6;
                case 7:
                    return Minesweeper.Properties.Resources.num7;
                case 8:
                    return Minesweeper.Properties.Resources.num8;
                default:
                    return Minesweeper.Properties.Resources.mine;
            }
        }

        private PictureBox[,] CreateGrid(int w, int h)
        {
            PictureBox[,] grid = new PictureBox[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    PictureBox pb = new PictureBox
                    {
                        Size = new Size(16, 16),
                        Location = new Point(i * 16, j * 16),
                        BackgroundImage = Minesweeper.Properties.Resources.tile,
                    };
                    grid[i, j] = pb;
                    pb.Click += Tile_Click;
                    pb.Tag = -1;
                    this.Controls.Add(pb);
                }
            }
            return grid;
        }

        private List<PictureBox> ValidIndicesAround(int xindex, int yindex, PictureBox[,] grid)
        {
            List<PictureBox> valid = new List<PictureBox>();

            Boolean inBoundsLeft = xindex > 0;
            Boolean inBoundsRight = xindex < WIDTH - 1;
            Boolean inBoundsTop = yindex > 0;
            Boolean inBoundsBottom = yindex < HEIGHT - 1;

            if (inBoundsTop)
            {
                if (inBoundsLeft) { valid.Add(grid[xindex - 1, yindex - 1]); }
                valid.Add(grid[xindex, yindex - 1]);
                if (inBoundsRight) { valid.Add(grid[xindex + 1, yindex - 1]); }
            }

            if (inBoundsBottom)
            {
                if (inBoundsLeft) { valid.Add(grid[xindex - 1, yindex + 1]); }
                valid.Add(grid[xindex, yindex + 1]);
                if (inBoundsRight) { valid.Add(grid[xindex + 1, yindex + 1]); }
            }

            if (inBoundsLeft) { valid.Add(grid[xindex - 1, yindex]); }

            if (inBoundsRight){ valid.Add(grid[xindex + 1, yindex]); }

            return valid;
        }

        private PictureBox[,] SetMines(int w, int h, int mines, PictureBox[,] grid)
        {
            Random r = new Random();
            while (mines > 0)
            {
                PictureBox pos = grid[r.Next() % w, r.Next() % h];
                if ((int)pos.Tag == -1)
                {
                    pos.Tag = -2;
                    pos.BackgroundImage = Minesweeper.Properties.Resources.mine;
                    mines--;
                }
            }
            return grid;
        }

        private void CountMinesBoard(int w, int h, PictureBox[,] grid)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (grid[i, j].Tag.Equals(-1))
                    {
                        CountMinesAroundCell(i, j, grid);
                    }
                }
            }
        }

        private void CountMinesAroundCell(int x, int y, PictureBox[,] grid)
        {
            int mines = 0;
            foreach (PictureBox p in this.ValidIndicesAround(x, y, grid))
            {
                if (p.Tag.Equals(-2)) { mines++; }
            }
            grid[x, y].Tag = mines;
            grid[x, y].BackgroundImage = this.MineTile(mines);
        }
    }
}
