namespace OthelloB.Lib
{
	public class PlayerAI : AbstractPlayer
	{
		private AIParameters parameters;

		public PlayerAI(Color c, AIParameters p) : base(c)
		{
			parameters = p;
		}

		public override Square GetMove(Board b)
		{
			Console.WriteLine("Finding AI move.");
			return MinimaxAB.GetBestMove(parameters, b, Color);
		}

		public override bool IsComputer() => true;
	}

	public static class MinimaxAB
	{
		private const int DEFAULT_DEPTH = 5;
		private const double LARGE_NUM = double.MaxValue / 1024;
		//TODO
		private static Dictionary<Board, double>[] _cache = new Dictionary<Board, double>[60];

		public static Square GetBestMove(AIParameters parameters, Board board, Color color)
		{
			//TODO Vary depth based on cache
			return _Explore(parameters, board, color, DEFAULT_DEPTH, -1 * LARGE_NUM, LARGE_NUM).move;
		}

		private static MoveScorePair _Explore(AIParameters p, Board b, Color c, int depth, double alpha, double beta)
		{
			//TODO Use cache to save data
			//Black wants to minimize score, white wants to maximize.
			if (b.IsGameOver())
			{
				//Check who wins, and give winner a large score (but not so large it says it can't find a move)
				int diff = b.GetTotalCount(Color.White) - b.GetTotalCount(Color.Black);
				if (b.GetWinner() == Color.None) return new MoveScorePair(new Square(), 0);
				else if (b.GetWinner() == Color.Black) return new MoveScorePair(new Square(), -1 * LARGE_NUM / 2 * (-diff / 64 + 1));
				else return new MoveScorePair(new Square(), LARGE_NUM / 2 * (diff / 64 + 1));
			}

			if (depth == 0)
			{
				double colorSign = c == Color.Black ? -1 : 1;
				Color c2 = b.GetOpposingColor(c);
				double score = colorSign * p.GetScore(b.GetMoveNumber(), b.GetTotalCount(c), b.GetMoveCount(c), b.GetStableCount(c));
				score += -colorSign * p.GetScore(b.GetMoveNumber(), b.GetTotalCount(c2), b.GetMoveCount(c2), b.GetStableCount(c2));
				return new MoveScorePair(new Square(), score);
			}

			//The initial "best" move is to lose
			MoveScorePair best = new MoveScorePair(new Square(), c == Color.Black ? LARGE_NUM : -1 * LARGE_NUM);
			LinkedList<Square> moves = b.GetValidMoves(c);
			Board running = new Board(b);

			//No moves, so we just swap to the next player without moving.
			if (moves.Count == 0) best = _Explore(p, running, b.GetOpposingColor(c), depth, alpha, beta);

			//If there aren't that many moves, we can count this as one depth.
			if (moves.Count <= 2 && depth < DEFAULT_DEPTH - 2) depth++;
			foreach (Square square in moves)
			{
				running.PlayMove(square.x, square.y, c);
				MoveScorePair childScore = _Explore(p, running, b.GetOpposingColor(c), depth - 1, alpha, beta);	
				running.LoadFrom(b);

				if (c == Color.White)
				{
					if (best.score < childScore.score)
					{
						best = childScore;
						best.move = new Square(square.x, square.y);
					}
					if (best.score > alpha) alpha = best.score;
				}
				else
				{
					if (best.score > childScore.score)
					{
						best = childScore;
						best.move = new Square(square.x, square.y);
					}
					if (best.score < beta) beta = best.score;
				}

				if (alpha >= beta) break;
			}

			if (depth == DEFAULT_DEPTH) Console.WriteLine("Best move found: " + best.move.ToString() + " with score " + best.score);
			return best;
		}

		private class MoveScorePair
		{
			public Square move;
			public double score;

			public MoveScorePair() : this(new Square(), 0) { }

			public MoveScorePair(Square m, double s)
			{
				move = m;
				score = s;
			}
		}
	}

	public class AIParameters
	{
		//Stores the curves for multipliers. In the format [b, m] for linear, [a, b, c] for quadratics, etc. (coefficients for increasing powers)
		private double[] _count;
		private double[] _mobility;
		private double[] _stability;

		public AIParameters(double[] c, double[] m, double[] s)
		{
			_count = c;
			_mobility = m;
			_stability = s;
		}

		public double GetScore(int move, int count, int mobility, int stability)
		{
			//TODO Cache powers/multipliers-by-turn for faster lookups
			double cMult = 0, mMult = 0, sMult = 0;
			for (int i = 0; i < _count.Length; i++) cMult += _count[i] * Math.Pow(move, i);
			for (int i = 0; i < _mobility.Length; i++) mMult += _mobility[i] * Math.Pow(move, i);
			for (int i = 0; i < _stability.Length; i++) sMult += _stability[i] * Math.Pow(move, i);

			return count * cMult + mobility * mMult + stability * sMult;
		}

		public static double[] FromPointsLinear(double x1, double y1, double x2, double y2)
		{
			double m = (y2 - y1) / (x2 - x1);
			double b = y1 - m * x1;
			return new[] { b, m };
		}
	}

}
