using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public class PCPlayer : Player
    {
        private IPlayerBrain iplayerBrain;
    
        public PCPlayer(XOSymbol xoSymbol)
            : base("Computer", xoSymbol)
        {
            iplayerBrain = new MinimaxPlayerBrain();
        }


        public IPlayerBrain IPlayerBrain
        {
            get
            {
                return this.iplayerBrain;
            }
            set
            {
                this.iplayerBrain = value;
            }
        }

        public override void Play()
        {
            GameMove gameMove = iplayerBrain.Think(GameEngine.GamePad, this.XOSymbol);
            this.GameMove = gameMove;
            GameEngine.MakeMove(this, gameMove);
        }
    }
}
