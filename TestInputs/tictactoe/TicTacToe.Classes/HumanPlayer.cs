using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(String name, XOSymbol xoSymbol)
            : base(name, xoSymbol)
        {

        }

        public override void Play()
        {
            GameEngine.MakeMove(this, GameMove);
        }
    }
}
