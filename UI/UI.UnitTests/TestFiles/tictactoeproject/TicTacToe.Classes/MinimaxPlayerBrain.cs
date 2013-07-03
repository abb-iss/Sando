using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public class MinimaxPlayerBrain : IPlayerBrain
    {

        XOSymbol playerSymbol;
        List<GamePadNode> listGamePadNode;

        #region IPlayerBrain Members

        public GameMove Think(XOSymbol[,] gamePad, XOSymbol playerSymbol)
        {
            this.playerSymbol = playerSymbol;
            List<GamePadNode> listGamePadNode = generateChilds(new GamePadNode(gamePad));
            this.listGamePadNode = listGamePadNode;
            foreach (GamePadNode node in listGamePadNode)
            {
                calculateHeuristic(node);
                if (node.IsWinning)
                    return node.GameMove;
            }

            //Find Best Heuristic
            int bestIndex = 0;
            GamePadNode _node;
            int best = 0;
            switch(playerSymbol)
            {
                case XOSymbol.X:
                    best = int.MinValue;
                    break;
                case XOSymbol.O:
                    best = int.MaxValue;
                    break;
            }

            for (int counter = 0; counter < listGamePadNode.Count; counter++ )
            {
                _node = listGamePadNode[counter];
                switch (playerSymbol)
                {
                    case XOSymbol.X: //Look for max value
                        if (_node.Heurisitic > best)
                        {
                            best = _node.Heurisitic;
                            bestIndex = counter;
                        }
                        break;
                    case XOSymbol.O: //Look for min Value
                        if (_node.Heurisitic < best)
                        {
                            best = _node.Heurisitic;
                            bestIndex = counter;
                        }
                        break;
                }
            }

            return listGamePadNode[bestIndex].GameMove;
        }

        public List<GamePadNode> GetGamePadNodes()
        {
            return this.listGamePadNode;
        }

        #endregion

        private List<GamePadNode> generateChilds(GamePadNode gamePadNode)
        {
            List<int> listEmptyCellsIndex;
            List<GamePadNode> listGamePadNode = new List<GamePadNode>();
            listEmptyCellsIndex = countEmptyCells(gamePadNode.GamePad);
            foreach (int currentIndex in listEmptyCellsIndex)
            {
                GamePadNode childGamePad = new GamePadNode();
                childGamePad.GamePad = (XOSymbol[,])gamePadNode.GamePad.Clone();
                childGamePad.GamePad.SetValue(playerSymbol, getRowIndex(currentIndex), getColumnIndex(currentIndex));
                childGamePad.GameMove = new GameMove(currentIndex, playerSymbol);
                listGamePadNode.Add(childGamePad);
            }
            return listGamePadNode;
        }

        private void calculateHeuristic(GamePadNode gamePadNode)
        {
            xFunction(gamePadNode);
            oFunction(gamePadNode);
        }

        private void xFunction(GamePadNode gamePadNode)
        {
            XOSymbol xoSymbol = XOSymbol.X;
            calculateRows(xoSymbol, gamePadNode);
            calculateColumns(xoSymbol, gamePadNode);
            calculateDiagonals(xoSymbol, gamePadNode);
        }

        private void oFunction(GamePadNode gamePadNode)
        {
            XOSymbol xoSymbol = XOSymbol.O;
            calculateRows(xoSymbol, gamePadNode);
            calculateColumns(xoSymbol, gamePadNode);
            calculateDiagonals(xoSymbol, gamePadNode);
        }

        private void calculateRows(XOSymbol xoSymbol, GamePadNode gamePadNode)
        {
            for (int row = 0; row < gamePadNode.GamePad.GetLength(0); row++)
            {
                List<XOSymbol> xoSymbolLine = new List<XOSymbol>();
                for (int column = 0; column < gamePadNode.GamePad.GetLength(0); column++)
                {
                    xoSymbolLine.Add((XOSymbol)gamePadNode.GamePad.GetValue(row, column));
                }
                if (isWinningMove(xoSymbol, xoSymbolLine))
                {
                    gamePadNode.IsWinning = true;
                    return;
                }
                if (hasPotential(xoSymbol, xoSymbolLine))
                {
                    switch (xoSymbol)
                    {
                        case XOSymbol.X:
                            gamePadNode.XFunction++;
                            break;
                        case XOSymbol.O:
                            gamePadNode.OFunction++;
                            break;
                    }
                }
            }
        }

        private void calculateColumns(XOSymbol xoSymbol, GamePadNode gamePadNode)
        {
            for (int column = 0; column < gamePadNode.GamePad.GetLength(0); column++)
            {
                List<XOSymbol> xoSymbolLine = new List<XOSymbol>();
                for (int row = 0; row < gamePadNode.GamePad.GetLength(0); row++)
                {
                    xoSymbolLine.Add((XOSymbol)gamePadNode.GamePad.GetValue(row, column));
                }
                if (isWinningMove(xoSymbol, xoSymbolLine))
                {
                    gamePadNode.IsWinning = true;
                    return;
                }
                if (hasPotential(xoSymbol, xoSymbolLine))
                {
                    switch (xoSymbol)
                    {
                        case XOSymbol.X:
                            gamePadNode.XFunction++;
                            break;
                        case XOSymbol.O:
                            gamePadNode.OFunction++;
                            break;
                    }
                }
            }
        }

        private void calculateDiagonals(XOSymbol xoSymbol, GamePadNode gamePadNode)
        {
            for (int diagonal = 0; diagonal < 2; diagonal++)
            {
                List<XOSymbol> xoSymbolLine = new List<XOSymbol>();
                for (int row = 0; row < gamePadNode.GamePad.GetLength(0); row++)
                {
                    int column = 0;

                    switch (diagonal)
                    {
                        case 0: column = row;
                            break;
                        case 1: column = (gamePadNode.GamePad.GetLength(0) - 1) - row;
                            break;
                    }
                    xoSymbolLine.Add((XOSymbol)gamePadNode.GamePad.GetValue(row, column));
                }
                if (isWinningMove(xoSymbol, xoSymbolLine))
                {
                    gamePadNode.IsWinning = true;
                    return;
                }
                if (hasPotential(xoSymbol, xoSymbolLine))
                {
                    switch (xoSymbol)
                    {
                        case XOSymbol.X:
                            gamePadNode.XFunction++;
                            break;
                        case XOSymbol.O:
                            gamePadNode.OFunction++;
                            break;
                    }
                }
            }
        }

        private List<int> countEmptyCells(XOSymbol[,] gamePad)
        {
            List<int> listEmptyCellsIndex = new List<int>();
            int index = 0;
            for (int row = 0; row < gamePad.GetLength(0); row++)
            {
                for (int column = 0; column < gamePad.GetLength(1); column++)
                {
                    if ((XOSymbol)gamePad.GetValue(row, column) == XOSymbol.EMPTY)
                    {
                        listEmptyCellsIndex.Add(index);
                    }
                    index++;
                }
            }
            return listEmptyCellsIndex;
        }

        /// <summary>
        /// Returns the row index from the GameMove position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private int getRowIndex(int position)
        {
            return position / 3;
        }

        /// <summary>
        /// Returns the column index from the GameMove position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private int getColumnIndex(int position)
        {
            return position % 3;
        }

        private bool isWinningMove(XOSymbol xoSymbol, List<XOSymbol> xoSymbolLine)
        {
            bool isMathced = true;
            foreach (XOSymbol xoSymbolCell in xoSymbolLine)
            {
                isMathced &= xoSymbolCell == xoSymbol;
            }
            return isMathced;
        }

        private bool hasPotential(XOSymbol xoSymbol, List<XOSymbol> xoSymbolLine)
        {
            bool isMatchedOrEmpty = true;
            foreach (XOSymbol xoSymbolCell in xoSymbolLine)
            {
                isMatchedOrEmpty &= (xoSymbolCell == xoSymbol || xoSymbolCell == XOSymbol.EMPTY);
            }
            return isMatchedOrEmpty;
        }

    }
}
