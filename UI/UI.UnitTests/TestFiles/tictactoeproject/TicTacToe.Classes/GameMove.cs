using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public class GameMove
    {
        private XOSymbol xoSymbol;
        private int position; //Valid positions are : 0,1,2 ... 7,8

        public GameMove(int position, XOSymbol xoSymbol)
        {
            if (position > 8 || position < 0)
                throw new Exception("Invlaid position. Valid Positions from Right to Left, Top Down is : 0,1,2,3,4,5,6,7,8");
            this.xoSymbol = xoSymbol;
            this.position = position;
        }
    
        public XOSymbol XOSymbol
        {
            get
            {
                return this.xoSymbol;
            }
        }

        public int Position
        {
            get
            {
                return this.position;
            }
        }

        /// <summary>
        /// Returns the row index from the GameMove position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int RowIndex
        {
            get
            {
                return position / 3;
            }
        }

        /// <summary>
        /// Returns the column index from the GameMove position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int ColumnIndex
        {
            get
            {
                return position % 3;
            }
        }
    }
}
