using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

public class Tile
{
    public enum State
    {
        unrevealed = 0,
        revealed,
        flag,
        qmark
    };

    public int Mines { get; set; } = 0;
    public State TileState { get; set; } = State.unrevealed;
    public bool IsMine { get; set; } = false;
    public PictureBox PB { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public List<Tile> ValidIndicesAround { get; set; }

    public Tile(int x, int y) 
	{
        this.X = x;
        this.Y = y;
	}
}