namespace OthelloB.Lib
{
	public class PlayerRandom : AbstractPlayer
	{
		private static Random _random = new Random();

		public PlayerRandom(Color c) : base(c) { }

		public override Square GetMove(Board b)
		{
			LinkedList<Square> moves = b.GetValidMoves(Color);
			if (moves.Count == 0) return new Square(-1, -1);
			int chosen = _random.Next(moves.Count);
			LinkedListNode<Square> f = moves.First;
			for (int i = 0; i < chosen; i++) f = f.Next;
			return f.Value;
		}
		
		public override bool IsComputer() => true;
	}
}
