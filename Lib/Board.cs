using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace OthelloB.Lib
{
	public class Board
	{
		//Board state
		private ulong _occupied;
		private ulong _owner;//Owner is 1 if white, 0 if black


		private ulong _stable;//1 if stable, 0 if not
		private bool _dirty;

		//State info cache
		//TODO Keep track of stability
		private int _bTotal, _bStable, _wTotal, _wStable;
		
		//Directions map
		private static readonly int[] _directions = {
			0, 1,
			1, 1,
			1, 0,
			-1, 1,
			0, -1,
			-1, -1,
			-1, 0,
			1, -1
		};
		
		public Board()
		{

			//Setup initial game state
			_occupied = 0;
			_owner = 0;

			_stable = 0;
			_dirty = false;

			_wTotal = 2;
			_bTotal = 2;
			_bStable = 0;
			_wStable = 0;

			_SetColor(3, 3, Color.Black);
			_SetColor(4, 4, Color.Black);

			_SetColor(4, 3, Color.White);
			_SetColor(3, 4, Color.White);
		}

		public Board(Board board)
		{
			//Clone the other board's info into this object
			LoadFrom(board);
		}

		public void LoadFrom(Board board)
		{
			_occupied = board._occupied;
			_owner = board._owner;

			_dirty = board._dirty;
			_stable = board._stable;

			_wTotal = board._wTotal;
			_bTotal = board._bTotal;
			_bStable = board._bStable;
			_wStable = board._wStable;
		}

		public bool IsGameOver()
		{
			//Console.WriteLine("Game Over Check: " + GetValidMoves(Color.Black).Count.ToString() + " | " + GetValidMoves(Color.White).Count.ToString());
			return GetValidMoves(Color.Black).Count == 0 && GetValidMoves(Color.White).Count == 0;
		}

		public Color GetWinner()
		{
			Console.WriteLine(_bTotal + " " + _wTotal);
			if (_bTotal == _wTotal) return Color.None;
			if (_bTotal < _wTotal) return Color.White;
			return Color.Black;
		}

		public int GetMoveCount(Color c)
		{
			//TODO Optimize by removing LL?
			return GetValidMoves(c).Count;
		}

		public int GetStableCount(Color c)
		{
			if (_dirty) _CalculateStability();

			if (c == Color.Black) return _bStable;
			else if (c == Color.White) return _wStable;
			return -1;
		}

		public int GetTotalCount(Color c)
		{
			if (c == Color.Black) return _bTotal;
			else if (c == Color.White) return _wTotal;
			return -1;
		}

		public int GetMoveNumber()
		{
			return _bTotal + _wTotal - 4;
		}

		public Color GetOpposingColor(Color c)
		{
			if (c == Color.None) return c;
			if (c == Color.Black) return Color.White;
			return Color.Black;
		}

		public Color GetColor(int i, int j)
		{
			return _GetColor(i, j);
		}

		public LinkedList<Square> GetValidMoves(Color c)
		{
			//TODO Instead of recalculating moves every time, we can instead update valid moves whenever a piece is placed
			LinkedList<Square> validMoves = new LinkedList<Square>();

			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{

					//Console.WriteLine("Checking square " + i + " " + j);
					//If square is taken, this isn't a valid move.
					if (_GetColor(i, j) != Color.None) continue;

					bool isValidMove = false;
					for (int d = 0; d < 8 && ! isValidMove; d++)
					{
						int dx = _directions[d * 2], dy = _directions[d * 2 + 1];

						//Console.WriteLine(dx + " " + dy + " " + _IsOnBoard(i + dx, j + dy) + " " + _squares.GetColor(i, j) + " " + _squares.GetColor(i + dx, j + dy));
						int index = 2;
						if (_IsOnBoard(i + dx, j + dy) && _IsOpponentColor(c,
							    _GetColor(i + dx, j + dy)))
						{
							//Console.WriteLine("Found neighboring enemy.");
							while (_IsOnBoard(i + dx * index, j + dy * index))
							{
								if (_GetColor(i + dx * index, j + dy * index) == c)
								{
									isValidMove = true;
									break;
								}
								if (_GetColor(i + dx * index, j + dy * index) == Color.None)
								{
									isValidMove = false;
									break;
								}

								index++;
							}

							if (isValidMove) validMoves.AddFirst(new Square(i, j));
						}
					}
					//if (isValidMove) Console.WriteLine("Square is valid move.");
				}
			}

			return validMoves;
		}

		public void PlayMove(int x, int y, Color c)
		{
			LinkedList<Square> flips = _GetFlips(x, y, c);
			_SetColor(x, y, c);
			foreach (Square s in flips) _SetColor(s.x, s.y, c);
		}

		//Private helper functions
		private LinkedList<Square> _GetFlips(int x, int y, Color c)
		{
			LinkedList<Square> res = new LinkedList<Square>();
			for (int d = 0; d < 8; d++)
			{
				int dx = _directions[d * 2], dy = _directions[d * 2 + 1];
				LinkedList<Square> cache = new LinkedList<Square>();
				int index = 1;
				while (_IsOnBoard(x + dx * index, y + dy * index))
				{
					Color cell = _GetColor(x + dx * index, y + dy * index);
					if (cell == Color.None) break;
					else if (_IsOpponentColor(cell, c)) cache.AddFirst(new Square(x + dx * index, y + dy * index));
					else
					{
						foreach (Square s2 in cache) res.AddFirst(s2);
						break;
					}
					index++;
				}
			}

			return res;
		}

		private void _CalculateStability()
		{
			_stable = 0;
			Queue<Square> queue = new Queue<Square>();
			if (_Occupied(0))
			{
				queue.Enqueue(new Square(0, 0));
				_SetStable(0);
			}
			if (_Occupied(7))
			{
				queue.Enqueue(new Square(0, 7));
				_SetStable(7);
			}
			if (_Occupied(56))
			{
				queue.Enqueue(new Square(7, 0));
				_SetStable(56);
			}
			if (_Occupied(63))
			{
				queue.Enqueue(new Square(7, 7));
				_SetStable(63);
			}

			while (queue.Count != 0)
			{
				Square s = queue.Dequeue();
				//Console.WriteLine("Investigating " + s.ToString());
				//Explore all children.
				for (int i = 0; i < _directions.Length; i += 2)
				{
					Square child = new Square(s.x + _directions[i], s.y + _directions[i + 1]);

					if (_IsOnBoard(child.x, child.y) && _Occupied(child.ToIndex()))
					{
						//Console.WriteLine("Found valid child " + child);
						//StableInDirection
						bool[] sid = new bool[8];
						for (int j = 0; j < _directions.Length; j += 2)
						{
							Square ns = new Square(child.x + _directions[j], child.y + _directions[j + 1]);
							sid[j / 2] = (!_IsOnBoard(ns.x, ns.y)) || (_Stable(ns.ToIndex()) && GetColor(child.x, child.y) == GetColor(ns.x, ns.y));
							//Console.WriteLine(_Stable(ns.ToIndex()) + " " + _IsOnBoard(ns.x, ns.y) + " " + (GetColor(child.x, child.y) == GetColor(ns.x, ns.y)));
						}
						//Console.WriteLine();
						//foreach (bool b in sid) Console.Write(b + " ");
						//Console.WriteLine();
						//Vertical, DiagUp, Horizontal, DiagDown
						bool stb = (sid[0] || sid[4]) && (sid[1] || sid[5]) && (sid[2] || sid[6]) && (sid[3] || sid[7]);
						if (stb && ! _Stable(child.ToIndex()))
						{
							_SetStable(child.ToIndex());
							if (_GetColor(child.x, child.y) == Color.Black) _bStable++;
							else _wStable++;
							queue.Enqueue(child);
						}
					}
				}
			}

			_dirty = false;
		}

		private bool _IsOnBoard(int x, int y)
		{
			return _IsOnBoard(x) && _IsOnBoard(y);
		}

		private bool _IsOnBoard(int a)
		{
			return a >= 0 && a < 8;
		}

		private bool _IsOpponentColor(Color c, Color target)
		{
			return c != Color.None && target != Color.None && c != target;
		}

		//Board State getters/setters
		private Color _GetColor(int x, int y)
		{
			int index = x * 8 + y;
			return !_Occupied(index) ? Color.None : _Owner(index) ? Color.White : Color.Black;
		}

		private void _SetColor(int x, int y, Color c)
		{
			Color old = _GetColor(x, y);

			//Set board states
			int index = x * 8 + y;
			if (c == Color.None)
			{
				if (old != Color.None) {
					if (old == Color.White) _wTotal--;
					else _bTotal--;
				}
				_occupied = _occupied & ~((ulong)1 << index);
				_dirty = true;
				return;
			}

			_occupied = _occupied | ((ulong)1 << index);
			if (c == Color.Black) _owner = _owner & ~((ulong)1 << index);
			else _owner = _owner | ((ulong)1 << index);

			//Update cache information
			if (old == Color.None)
			{
				if (c == Color.Black) _bTotal++;
				else if (c == Color.White) _wTotal++;
			}
			else if (c != old)
			{
				if (c == Color.Black)
				{
					_bTotal++;
					_wTotal--;
				}
				else if (c == Color.White)
				{
					_wTotal++;
					_bTotal--;
				}
				_dirty = true;
			}
		}

		private bool _Occupied(int i)
		{
			return (_occupied & ((ulong)1 << i)) != 0;
		}

		private bool _Stable(int i)
		{
			return (_stable & ((ulong)1 << i)) != 0;
		}

		private void _SetStable(int i)
		{
			_stable = _stable | ((ulong)1 << i);
		}

		private bool _Owner(int i)
		{
			return (_owner & ((ulong)1 << i)) != 0;
		}

		//Hashing functions
		public override bool Equals(object obj)
		{
			var item = obj as Board;

			if (item == null) return false;

			return _owner == item._owner && _occupied == item._occupied;
		}

		public override int GetHashCode()
		{
			return (int)_owner & (int)(_occupied >> 32);
		}

		public void _DebugPrintStability()
		{
			_CalculateStability();
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					Console.Write(_Occupied(i * 8 + j) ? _Stable(i * 8 + j) ? "O" : "o" : ".");
				}
				Console.WriteLine();
			}
		}
	}

	public struct Square
	{
		public int x, y;

		public Square() : this(-1, -1) {}

		public Square(int i, int j)
		{
			x = i;
			y = j;
		}

		public int ToIndex()
		{
			return x * 8 + y;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Square)) return false;

			Square s = (Square)obj;
			// compare elements here
			return s.x == x && s.y == y;
		}

		public override int GetHashCode()
		{
			return x * 10 + y;
		}

		public override string ToString()
		{
			return "(" + x.ToString() + ", " + y.ToString()	+ ")";
		}
	}

	public enum Color
	{
		None,
		Black,
		White
	}
}
