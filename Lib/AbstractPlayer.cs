namespace OthelloB.Lib
{
	public abstract class AbstractPlayer
	{
		public readonly Color Color;

		public AbstractPlayer(Color c)
		{
			Color = c;
		}

		public abstract Square GetMove(Board b);

		public abstract bool IsComputer();
	}
}
