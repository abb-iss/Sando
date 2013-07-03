using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public class GamePadNode
    {

        public GamePadNode()
        {
        }

        public GamePadNode(XOSymbol[,] gamePad)
        {
            GamePad = gamePad;
        }

        private XOSymbol[,] gamePad;
        private int xFunction;
        private int oFunction;
        private bool isWinning;
        private GameMove gameMove;

        public XOSymbol[,] GamePad
        {
            get
            {
                return gamePad;
            }
            set
            {
                this.gamePad = value;
            }
        }

        public int XFunction
        {
            get
            {
                return xFunction;
            }
            set
            {
                this.xFunction = value;
            }
        }

        public int OFunction
        {
            get
            {
                return oFunction;
            }
            set
            {
                this.oFunction = value;
            }
        }

        public int Heurisitic
        {
            get
            {
                return this.xFunction - this.oFunction;
            }
        }

        public bool IsWinning
        {
            get
            {
                return this.isWinning;
            }
            set
            {
                this.isWinning = value;
            }
        }

        public GameMove GameMove
        {
            get
            {
                return this.gameMove;
            }
            set
            {
                this.gameMove = value;
            }
        }
    }
}
