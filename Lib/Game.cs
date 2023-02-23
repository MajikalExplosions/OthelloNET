
namespace OthelloB.Lib
{
    public class Game
    {
	    public Board Board;
	    public AbstractPlayer Black, White;
	    public Color Turn;

	    public Game(AbstractPlayer b, AbstractPlayer w)
	    {
		    Board = new Board();
		    Black = b;
		    White = w;
		    Turn = Color.Black;
	    }

	    public LinkedList<Square> GetValidMoves()
	    {
		    return Board.GetValidMoves(Turn);
	    }

	    public string GetDrawColor(int x, int y)
	    {
		    if (Board.GetColor(x, y) == Color.None) return "darkgreen";
		    if (Board.GetColor(x, y) == Color.White) return "white";
		    return "black";
	    }

	    public bool RunHumanTurn(Square s)
	    {
		    AbstractPlayer player = Turn == Color.Black ? Black : White;
		    Turn = Board.GetOpposingColor(Turn);
		    if (Board.GetValidMoves(player.Color).Contains(s))
		    {
				Board.PlayMove(s.x, s.y, player.Color);
				return true;
		    }

		    return false;
	    }

	    public bool RunAITurn()
	    {
			Console.WriteLine("Running AI turn.");
		    AbstractPlayer player = Turn == Color.Black ? Black : White;
		    if ((Turn == Color.Black && Black.IsComputer()) || (Turn == Color.White && White.IsComputer()))
			{
				Turn = Board.GetOpposingColor(Turn);
				Square move = player.GetMove(Board);
			    if (move.x != -1)
			    {
				    Board.PlayMove(move.x, move.y, player.Color);
					return true;
				}
		    }

		    return false;
	    }

	    public string GetWinnerString()
	    {
		    return (Board.GetWinner() == Color.None ? "Tie" : Board.GetWinner() == Color.Black ? "Black" : "White");
	    }

	    public bool IsGameOver()
	    {
		    return Board.IsGameOver();
	    }
    }

}
