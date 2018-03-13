using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

//Java package equivalent.
namespace Minesweeper
{
    public partial class MainForm : Form
    {
        //Width and Height of the board
        //In C# const's are implicitly static, and behave like static finals in Java.
        private const int WIDTH = 30;
        private const int HEIGHT = 16;

        //There isn't a check yet for MINES < WIDTH * HEIGHT so modify this with caution.
        private const int MINES = 99; 

        //Size in pixels of each tile.
        private const int TILE_SIZE = 16;

        //Horizontal and Vertical margin in order to have the form tightly wrap the board.
        private const int HMARGIN = 16;
        private const int VMARGIN = 39;

        //Named resources. C# forms have a neat 'sprite-sheet'
        //interface called Resources.resx under the properites of each solution (project).

        //Readonly's in C# are similar to consts but because they're
        //not static, and you can't garuntee they won't be changed by
        //another program, they must be assigned readonly.
        private readonly Image TILE = Properties.Resources.tile;
        private readonly Image FLAG = Properties.Resources.flag;
        private readonly Image QMARK = Properties.Resources.unknown;
        private readonly Image MINE_HIT = Properties.Resources.mine_hit;
        private readonly Image MINE = Properties.Resources.mine;
        private readonly Image MINE_MISS = Properties.Resources.mine_wrong;

        //Grid to be filled with tiles and mines. 
        //Multidimensional array declaration in C# is a little weird.
        //Integer[][] -> Integer[,]; String[][][] -> String[,,];
        //And you can reference positions like:
        //int i = array[x,y] or string s = array[x,y,z]
        private Tile[,] grid;

        //stopped prevents any clicks to be performed on the grid when true
        private bool stopped = false;

        //opened allows for 'useful' first click's when false. 
        private bool opened = false;

        //Regular constructor
        public MainForm()
        {
            //Initializes form in MainForm.Designer.cs
            InitializeComponent();

            //Windows Form objects are called controls, and
            //most controls have specific classes for their properties,
            //like Size.
            Size = new Size(WIDTH * TILE_SIZE + HMARGIN, 
                                 HEIGHT * TILE_SIZE + VMARGIN);

            //Without this line, the form would be built from
            //the top left of the screen.
            CenterToScreen();

            grid = CreateGrid(WIDTH, HEIGHT);
            SetValidities(WIDTH, HEIGHT);

            if (opened)
            {
                SetMines(WIDTH, HEIGHT, MINES);
            }
        }

        //C# methods are by convention UpperCamelCase

        /// <summary>
        /// Calls CheckIndices() on each cell, and assigns its value to each cell's ValidIndicesAround list.
        /// </summary>
        /// <param name="w">Width of the grid.</param>
        /// <param name="h">Height of the grid.</param>
        public void SetValidities(int w, int h)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    grid[i, j].ValidIndicesAround = CheckIndices(i, j);
                }
            }
        }

        /// <summary>
        /// Checks a cell for its valid adjacent indices.
        /// </summary>
        /// <param name="x">x index of tile to check</param>
        /// <param name="y">y index of tile to check</param>
        /// <returns>A list of valid adjacent indices.</returns>
        private List<Tile> CheckIndices(int x, int y)
        {
            List<Tile> valid = new List<Tile>();

            bool inBoundsLeft = x > 0;
            bool inBoundsRight = x < WIDTH - 1;
            bool inBoundsTop = y > 0;
            bool inBoundsBottom = y < HEIGHT - 1;

            if (inBoundsTop)
            {
                if (inBoundsLeft) {
                    valid.Add(grid[x - 1, y - 1]);
                }

                valid.Add(grid[x, y - 1]);

                if (inBoundsRight) {
                    valid.Add(grid[x + 1, y - 1]);
                }
            }

            if (inBoundsBottom)
            {
                if (inBoundsLeft) {
                    valid.Add(grid[x - 1, y + 1]);
                }

                valid.Add(grid[x, y + 1]);

                if (inBoundsRight) {
                    valid.Add(grid[x + 1, y + 1]);
                }
            }

            if (inBoundsLeft) {
                valid.Add(grid[x - 1, y]);
            }

            if (inBoundsRight) {
                valid.Add(grid[x + 1, y]);
            }

            return valid;
        }

        /// <summary>
        /// Creates a grid of Tiles and adds each PictureBox control to the form.
        /// </summary>
        /// <param name="w">Width of the grid.</param>
        /// <param name="h">Height of the grid.</param>
        /// <returns>A grid of tiles.</returns>
        public Tile[,] CreateGrid(int w, int h)
        {
            Tile[,] newGrid = new Tile[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Tile tile = new Tile(i, j);

                    //PictureBox's are the basic image holder control.
                    //Any class with 'set;' variables can be declared with
                    //a bunch of its values like this.
                    tile.PB = new PictureBox()
                    {
                        Size = new Size(TILE_SIZE, TILE_SIZE),
                        Location = new Point(tile.X * TILE_SIZE, tile.Y * TILE_SIZE),
                        BackgroundImage = TILE,

                        //Each control has an object variable called Tag.
                        //In this case, Tag self-refers to the Tile that
                        //contains the PictureBox.
                        Tag = tile
                    };                    

                    newGrid[i, j] = tile;
                    
                    //Subscribes the method Mine_Mouse_Up to the
                    //event listener 'MouseUp' of the PictureBox.
                    //This line is super important, and its how you
                    //programatically subscribe any listener.
                    tile.PB.MouseUp += Mine_Mouse_Up;

                    //Adds the PictureBox to the form's controls,
                    //effectively making it live.
                    Controls.Add(tile.PB);
                }
            }
            return newGrid;
        }

        /// <summary>
        /// Handler for MouseUp events on a mine tile.
        /// </summary>
        /// <param name="sender">Object that sent the event (PictureBox in this case).</param>
        /// <param name="e">Data associated with event. (Button, number of clicks, scroll delta, etc).</param>
        public void Mine_Mouse_Up(object sender, MouseEventArgs e)
        {
            if (stopped) { return; }
            PictureBox pb = sender as PictureBox;                     
            if (e.Button == MouseButtons.Left) {
                HandleLeftClick((Tile) pb.Tag);                
            } else if (e.Button == MouseButtons.Right)
            {
                HandleRightClick((Tile) pb.Tag);
            }
        }

        /// <summary>
        /// Helper method to handle left clicks on tiles.
        /// </summary>
        /// <param name="tile">Tile that was clicked.</param>
        private void HandleLeftClick(Tile tile)
        {
            List<Tile> cellsToOpen = new List<Tile>();
            if (!opened) {
                SetMines(tile, WIDTH, HEIGHT, MINES);
            }

            //Uses enum 'State' from Tile class.
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
                OpenCells(cellsToOpen);
            } else if (tile.TileState == Tile.State.revealed)
            {
                ChordCheck(tile);
            }
        }

        /// <summary>
        /// Helper method to handle right clicks on tiles. Cycles states.
        /// </summary>
        /// <param name="tile">Tile that was clicked.</param>
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

        /// <summary>
        /// Ends the game.
        /// </summary>
        public void EndGame()
        {
            stopped = true;
            RevealMines();
        }

        /// <summary>
        /// Reveals unfound mines and reveals flag misses.
        /// </summary>
        public void RevealMines()
        {
            foreach (Tile t in grid)
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

        /// <summary>
        /// Counts the number of adjacent mines.
        /// </summary>
        /// <param name="centerCell">Tile to check around.</param>
        /// <returns>Number of adjacent mines.</returns>
        private int CountMinesAroundCell(Tile centerCell)
        {
            int mines = 0;
            foreach (Tile t in centerCell.ValidIndicesAround)
            {
                if (t.IsMine) { mines++; }
            }
            return mines;
        }

        /// <summary>
        /// Checks if a chord is possible, and if it is, calls the Chord method.
        /// </summary>
        /// <param name="tile">Tile to check.</param>
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

        /// <summary>
        /// Handles Chording. A chord in minesweeper is when you have a scenario like below:
        /// 
        /// ?!?  where question marks are unrevealed tiles
        /// ?1?  and exclamation points are flags.
        /// ???
        /// 
        /// and left-clicking the known tile will reveal around the tile like so:
        /// 
        /// 0!0
        /// 010
        /// 000
        /// 
        /// So long as there are as many adjacent flags to the tile as there are mines.
        /// </summary>
        /// <param name="tile">The tile to chord.</param>
        private void Chord(Tile tile)
        {
            List<Tile> mines = new List<Tile>();
            foreach (Tile t in tile.ValidIndicesAround)
            {
                if (t.IsMine && t.TileState != Tile.State.flag)
                {
                    mines.Add(t);
                } else if (t.TileState == Tile.State.unrevealed || t.TileState == Tile.State.qmark)
                {
                    HandleLeftClick(t);
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

        /// <summary>
        /// Handles opening chains of empty tiles.
        /// </summary>
        /// <param name="cellsToOpen">List of cells to handle opening.</param>
        private void OpenCells(List<Tile> cellsToOpen)
        {
            while (cellsToOpen.Any())
            {
                Tile open = cellsToOpen[0];

                int mines = CountMinesAroundCell(open);
                open.Mines = mines;
                open.TileState = Tile.State.revealed;
                open.PB.BackgroundImage = AdjacencyTile(mines);

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

        /// <summary>
        /// Helper method for retrieving numbered tiles.
        /// </summary>
        /// <param name="i">Number of adjacent mines.</param>
        /// <returns>The image of the cooresponding tile.</returns>
        private Image AdjacencyTile(int i)
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
                    return Minesweeper.Properties.Resources.tile;
            }
        }

        /// <summary>
        /// Places all mines on the grid.
        /// </summary>
        /// <param name="center">The tile that was first clicked.</param>
        /// <param name="w">Width of the grid.</param>
        /// <param name="h">Height of the grid.</param>
        /// <param name="mines">Number of mines to set.</param>
        public void SetMines(Tile center, int w, int h, int mines)
        {
            Random r = new Random();
            while (mines > 0)
            {
                int x = r.Next() % w;
                int y = r.Next() % h;
                Tile tile = grid[x, y];
                if (!tile.IsMine && !center.ValidIndicesAround.Contains(tile) && tile != center)
                {
                    tile.IsMine = true;
                    tile.PB.BackgroundImage = TILE;
                    mines--;
                }
            }
            opened = true;
        }

        /// <summary>
        /// Overload method in case 'opened' is true from the start.
        /// </summary>
        /// <param name="w">Width of the grid.</param>
        /// <param name="h">Height of the grid.</param>
        /// <param name="mines">Number of mines to set.</param>
        private void SetMines(int w, int h, int mines)
        {
            Random r = new Random();
            while (mines > 0)
            {
                int x = r.Next() % w;
                int y = r.Next() % h;
                Tile tile = grid[x, y];
                if (!tile.IsMine)
                {
                    tile.IsMine = true;
                    tile.PB.BackgroundImage = TILE;
                    mines--;
                }
            }
        }

        /// <summary>
        /// Resets the board.
        /// </summary>
        public void ResetBoard()
        {
            foreach (Tile t in grid)
            {
                t.Mines = 0;
                t.IsMine = false;
                t.TileState = Tile.State.unrevealed;
                t.PB.BackgroundImage = TILE;
            }

            opened = false;
            stopped = false;
        }

        /// <summary>
        /// Handler for KeyDown event on the entire form.
        /// </summary>
        /// <param name="sender">Object that sent the event (Form in this case).</param>
        /// <param name="e">Data associated with event.</param>
        public void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R)
            {
                ResetBoard();
            }
        }
    }
}
