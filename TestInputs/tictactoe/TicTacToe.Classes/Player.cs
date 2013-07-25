using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public abstract class Player
    {
        private String name;
        private XOSymbol xoSymbol;
        private GameMove gameMove;
        private GameEngine gameEngine;

        public Player(string name, XOSymbol xoSymbol)
        {
            this.name = name;
            this.xoSymbol = xoSymbol;
        }

        public GameMove GameMove
        {
            get
            {
                return this.gameMove;
            }
            set
            {
                if (value.XOSymbol != this.XOSymbol)
                    throw new Exception("Invalid XOSymbol, becuase the player symbol is : " + xoSymbol);
                this.gameMove = value;
            }
        }

        public GameEngine GameEngine
        {
            get
            {
                return gameEngine;
            }
            set
            {
                this.gameEngine = value;
            }
        }
        
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public XOSymbol XOSymbol
        {
            get { return xoSymbol; }
            set { xoSymbol = value; }
        }
        
        public abstract void Play();
    }
}
