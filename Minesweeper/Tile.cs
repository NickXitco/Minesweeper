using System.Collections.Generic;
using System.Windows.Forms;

public class Tile
{
    /// <summary>
    /// enums are basically like integer aliases.
    /// The first entry starts at 1 and counts up, and each
    /// entry can be called with Tile.State.entry, and be compared
    /// to like an integer.
    /// </summary>
    public enum State
    {
        unrevealed,
        revealed,
        flag,
        qmark
    };

    //C# doesn't need getters and setters, just to be declared with them.
    public int Mines { get; set; } = 0;
    public State TileState { get; set; } = State.unrevealed;
    public bool IsMine { get; set; } = false;
    public PictureBox PB { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public List<Tile> ValidIndicesAround { get; set; }

    public Tile(int x, int y) 
	{
        X = x;
        Y = y;
	}
}