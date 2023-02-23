namespace OthelloB.Lib
{
	public class PlayerHuman : AbstractPlayer
	{
		//This is just a placeholder class for Game; the actual GetMove never gets called.
		public PlayerHuman(Color c) : base(c)
		{
		}

		public override Square GetMove(Board b)
		{
			return new Square();
		}

		public override bool IsComputer() => false;
	}
}
