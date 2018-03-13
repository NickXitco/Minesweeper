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
        private const int WIDTH = 50;
        private const int HEIGHT = 20;

        private const int MINES = 250;

        private const int TILE_SIZE = 16;

        private const int HMARGIN = 30;
        private const int VMARGIN = 50;

        private const int PROXIMITY_MAX = 9;

        private readonly Image TILE = Properties.Resources.tile;
        private readonly Image FLAG = Properties.Resources.flag;
        private readonly Image QMARK = Properties.Resources.unknown;
        private readonly Image MINE_HIT = Properties.Resources.mine_hit;
        private readonly Image MINE = Properties.Resources.mine;
        private readonly Image MINE_MISS = Properties.Resources.mine_wrong;

        private Tile[,] grid;
        private bool stopped = false;
        private bool opened = false;

        public Form1()
        {            
            InitializeComponent();
            this.Size = new Size(WIDTH * TILE_SIZE + HMARGIN, 
                                 HEIGHT * TILE_SIZE + VMARGIN);
            this.CenterToScreen();
            this.grid = this.CreateGrid(WIDTH, HEIGHT);
            this.SetValidities(WIDTH, HEIGHT);
        }

        private void SetValidities(int w, int h)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    this.grid[i, j].ValidIndicesAround = this.CheckInices(i, j);
                }
            }
        }

        private List<Tile> CheckInices(int xindex, int yindex)
        {
            List<Tile> valid = new List<Tile>();

            Boolean inBoundsLeft = xindex > 0;
            Boolean inBoundsRight = xindex < WIDTH - 1;
            Boolean inBoundsTop = yindex > 0;
            Boolean inBoundsBottom = yindex < HEIGHT - 1;

            if (inBoundsTop)
            {
                if (inBoundsLeft) { valid.Add(this.grid[xindex - 1, yindex - 1]); }
                valid.Add(this.grid[xindex, yindex - 1]);
                if (inBoundsRight) { valid.Add(this.grid[xindex + 1, yindex - 1]); }
            }

            if (inBoundsBottom)
            {
                if (inBoundsLeft) { valid.Add(this.grid[xindex - 1, yindex + 1]); }
                valid.Add(this.grid[xindex, yindex + 1]);
                if (inBoundsRight) { valid.Add(this.grid[xindex + 1, yindex + 1]); }
            }

            if (inBoundsLeft) { valid.Add(this.grid[xindex - 1, yindex]); }

            if (inBoundsRight) { valid.Add(this.grid[xindex + 1, yindex]); }

            return valid;
        }


        private Tile[,] CreateGrid(int w, int h)
        {
            Tile[,] newGrid = new Tile[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Tile tile = new Tile(i, j);
                    tile.PB = new PictureBox()
                    {
                        Size = new Size(TILE_SIZE, TILE_SIZE),
                        Location = new Point(tile.X * TILE_SIZE, tile.Y * TILE_SIZE),
                        BackgroundImage = TILE,
                        Tag = tile
                    };                    

                    newGrid[i, j] = tile;
                    tile.PB.MouseUp += this.Mine_Mouse_Up;
                    this.Controls.Add(tile.PB);
                }
            }
            return newGrid;
        }

        
        private void Mine_Mouse_Up(object sender, MouseEventArgs e)
        {
            if (this.stopped) { return; }
            PictureBox pb = sender as PictureBox;
            if (!this.opened) { this.grid = this.SetMines((Tile) pb.Tag, WIDTH, HEIGHT, MINES); }            
            if (e.Button == MouseButtons.Left) {
                this.HandleLeftClick((Tile) pb.Tag);                
            } else if (e.Button == MouseButtons.Right)
            {
                this.HandleRightClick((Tile) pb.Tag);
            }
        }

        private void HandleLeftClick(Tile tile)
        {
            List<Tile> cellsToOpen = new List<Tile>();

            if (tile.TileState == Tile.State.unrevealed || tile.TileState == Tile.State.qmark)
            {
                if (tile.IsMine)
                {
                    tile.TileState = Tile.State.revealed;
                    tile.PB.BackgroundImage = MINE_HIT;
                    EndGame();
                    return;
                }
                cellsToOpen.Add(tile);
                this.OpenCells(cellsToOpen);
            } else if (tile.TileState == Tile.State.revealed)
            {
                ChordCheck(tile);
            }
        }

        private void EndGame()
        {
            stopped = true;
            this.RevealMines();
        }

        private void RevealMines()
        {
            foreach (Tile t in this.grid)
            {
                if (t.IsMine && t.TileState == Tile.State.unrevealed)
                {
                    t.TileState = Tile.State.revealed;
                    t.PB.BackgroundImage = MINE;
                } else if (!t.IsMine && t.TileState == Tile.State.flag)
                {
                    t.TileState = Tile.State.revealed;
                    t.PB.BackgroundImage = MINE_MISS;
                }
            }
        }

        private void ChordCheck(Tile tile)
        {
            if (tile.Mines < 1) { return; }
            int flagCount = 0;

            foreach(Tile t in tile.ValidIndicesAround)
            {
                if (t.TileState == Tile.State.flag)
                {
                    flagCount++;
                }
            }

            if (flagCount == tile.Mines)
            {
                Chord(tile);
            }
        }

        private void Chord(Tile tile)
        {
            List<Tile> mines = new List<Tile>();
            foreach (Tile t in tile.ValidIndicesAround)
            {
                if (t.IsMine && t.TileState != Tile.State.flag)
                {
                    mines.Add(t);
                } else if (t.TileState == Tile.State.unrevealed)
                {
                    this.HandleLeftClick(t);
                }
            }

            if (mines.Any())
            {
                EndGame();
            }

            foreach (Tile t in mines)
            {
                t.TileState = Tile.State.revealed;
                t.PB.BackgroundImage = MINE_HIT;
            }
        }

        private void HandleRightClick(Tile tile)
        {
            switch (tile.TileState)
            {
                case Tile.State.unrevealed:
                    tile.PB.BackgroundImage = FLAG;
                    tile.TileState = Tile.State.flag;
                    break;
                case Tile.State.flag:
                    tile.PB.BackgroundImage = QMARK;
                    tile.TileState = Tile.State.qmark;
                    break;
                case Tile.State.qmark:
                    tile.PB.BackgroundImage = TILE;
                    tile.TileState = Tile.State.unrevealed;
                    break;
            }
        }
        

        private void OpenCells(List<Tile> cellsToOpen)
        {
            while (cellsToOpen.Any())
            {
                Tile open = cellsToOpen[0];

                int mines = this.CountMinesAroundCell(open);
                open.Mines = mines;
                open.TileState = Tile.State.revealed;
                open.PB.BackgroundImage = this.MineTile(mines);

                if (mines == 0)
                {
                    foreach (Tile t in open.ValidIndicesAround)
                    {
                        if (t.TileState == Tile.State.unrevealed && !cellsToOpen.Contains(t))
                        {
                            cellsToOpen.Add(t);
                        }
                    }
                }
                cellsToOpen.RemoveAt(0);
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

        private Tile[,] SetMines(Tile center, int w, int h, int mines)
        {
            Random r = new Random();
            while (mines > 0)
            {
                int x = r.Next() % w;
                int y = r.Next() % h;
                Tile tile = this.grid[x, y];
                if (!tile.IsMine && !center.ValidIndicesAround.Contains(tile) && tile != center)
                {
                    tile.IsMine = true;
                    tile.PB.BackgroundImage = TILE;
                    mines--;
                }
            }
            this.opened = true;
            return this.grid;
        }

        private void CountMinesBoard(int w, int h)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (this.grid[i, j].TileState == Tile.State.unrevealed)
                    {
                        CountMinesAroundCell(this.grid[i, j]);
                    }
                }
            }
        }

        private int CountMinesAroundCell(Tile centerCell)
        {
            int mines = 0;
            foreach (Tile t in centerCell.ValidIndicesAround)
            {
                if (t.IsMine) { mines++; }
            }
            return mines;
        }
    }
}
