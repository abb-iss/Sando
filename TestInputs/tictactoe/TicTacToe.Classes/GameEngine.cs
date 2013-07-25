using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Classes
{
    public class GameEngine
    {
        private HumanPlayer humanPlayer;
        private PCPlayer pcPlayer;
        private Player currentPlayerTurn;
        private XOSymbol[,] gamePad;
        private Player winnerPlayer;
        private bool isGameOver;

        public GameEngine(HumanPlayer humanPlayer, PCPlayer pcPlayer, Player firstPlayerTurn)
        {
            if (humanPlayer.XOSymbol == pcPlayer.XOSymbol)
                throw new Exception("Invalid Players.You can not give the PC and the Human player the same XO symbol");

            HumanPlayer = humanPlayer;
            PCPlayer = pcPlayer;
            this.currentPlayerTurn = firstPlayerTurn;
            gamePad = new XOSymbol[3, 3];
            winnerPlayer = null;
            isGameOver = false;
        }

        public Player CurrentPlayerTurn
        {
            get
            {
                return currentPlayerTurn; ;
            }
        }

        public XOSymbol[,] GamePad
        {
            get
            {
                return (XOSymbol[,])gamePad.Clone();
            }
        }

        public bool IsGameOver
        {
            get
            {
                return isGameOver;
            }
        }

        public Player WinnerPlayer
        {
            get
            {
                return winnerPlayer;
            }
        }

        public HumanPlayer HumanPlayer
        {
            get
            {
                return humanPlayer;
            }
            set
            {
                this.humanPlayer = value;
                this.humanPlayer.GameEngine = this;
            }
        }

        public PCPlayer PCPlayer
        {
            get
            {
                return pcPlayer;
            }
            set
            {
                this.pcPlayer = value;
                this.pcPlayer.GameEngine = this;
            }
        }

        public bool MakeMove(Player player, GameMove gameMove)
        {
            if (!validateGameMove(player, gameMove))
            {
                return false;
            }
            //Write the move in the GamePad
            int rowIndex, columnIndex;
            rowIndex = getRowIndex(gameMove.Position);
            columnIndex = getColumnIndex(gameMove.Position);
            gamePad.SetValue(gameMove.XOSymbol, rowIndex, columnIndex);

            //Switch the player turns
            switchPlayers();

            //Process Game status to see whether there is a Winner, or the game ends
            processGame();

            //Drive the PCPlayer to Play
            if (currentPlayerTurn.Equals(PCPlayer) && !IsGameOver)
                pcPlayer.Play();
            return true;
        }

        /// <summary>
        /// Rules to be validated
        /// 1- Game is not over AND
        /// 2- The player own this turn AND
        /// 3- The XOSymbol position is valid
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gameMove"></param>
        /// <returns></returns>
        private bool validateGameMove(Player player, GameMove gameMove)
        {
            return (!IsGameOver && player.Equals(currentPlayerTurn) && validatePosition(gameMove));
        }

        /// <summary>
        /// Rules:
        /// 1- The Position does not have already an Symbol
        /// </summary>
        /// <param name="gameMove"></param>
        /// <returns></returns>
        private bool validatePosition(GameMove gameMove)
        {
            int rowIndex, columnIndex;
            rowIndex = getRowIndex(gameMove.Position);
            columnIndex = getColumnIndex (gameMove.Position);
            return (XOSymbol)gamePad.GetValue(rowIndex, columnIndex) == XOSymbol.EMPTY;
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

        /// <summary>
        /// Switch between players.
        /// For instnace if the Human played, it will give the turn to PC
        /// </summary>
        private void switchPlayers()
        {
            if (currentPlayerTurn.Equals(PCPlayer))
                currentPlayerTurn = HumanPlayer;
            else
                currentPlayerTurn = PCPlayer;
        }

        /// <summary>
        /// Check whether:
        /// 1- There is a winner OR
        /// 2- The game is finished with tie (no winner)
        /// </summary>
        private void processGame()
        {   
            //Check if there is a winner
            checkRows();
            if (IsGameOver)
                return;
            checkColumns();
            if (IsGameOver)
                return;
            checkDiagonals();
            if (IsGameOver)
                return;
            checkGameOver();
        }

        /// <summary>
        /// Check if there is a winner based on Row Line 
        /// Sample Data : 
        /// X X X 
        /// </summary>
        private void checkRows()
        {
            for (int row = 0; row < gamePad.GetLength(0); row++)
            {
                XOSymbol cellXOSymbol = (XOSymbol)gamePad.GetValue(row, 0); //Get first element of the current Row
                bool lineMatched = cellXOSymbol != XOSymbol.EMPTY; //Ensure that it is not empty cell
                for (int column = 0; column < gamePad.GetLength(1); column++)
                {
                    lineMatched &= (cellXOSymbol == (XOSymbol)gamePad.GetValue(row, column));
                    if (!lineMatched)
                        break;
                }
                if (lineMatched)
                    endGame(cellXOSymbol);
            }
        }

        /// <summary>
        /// Check if there is a winner based on Column Line 
        /// Sample Data : 
        /// X
        /// X
        /// X
        /// </summary>
        private void checkColumns()
        {
            for (int column = 0; column < gamePad.GetLength(0); column++)
            {
                XOSymbol cellXOSymbol = (XOSymbol)gamePad.GetValue(0, column); //Get first element of the current Column
                bool lineMatched = cellXOSymbol != XOSymbol.EMPTY; //Ensure that it is not empty cell
                for (int row = 0; row < gamePad.GetLength(1); row++)
                {
                    lineMatched &= (cellXOSymbol == (XOSymbol)gamePad.GetValue(row, column));
                    if (!lineMatched)
                        break;
                }
                if (lineMatched)
                    endGame(cellXOSymbol);
            }
        }

        /// <summary>
        /// /// <summary>
        /// Check if there is a winner based on Diagonal Line 
        /// Sample Data : 
        /// X 
        ///   X 
        ///     X 
        /// </summary>
        /// </summary>
        private void checkDiagonals()
        {
            for (int diagonal = 0; diagonal < 2; diagonal++)
            {
                XOSymbol cellXOSymbol = (XOSymbol)gamePad.GetValue(diagonal, diagonal); //Get an element of the current Diagonal
                bool lineMatched = cellXOSymbol != XOSymbol.EMPTY; //Ensure that it is not empty cell
                for (int row = 0; row < gamePad.GetLength(0); row++)
                {
                    int column = 0;
                    
                    switch (diagonal)
                    {
                        case  0:
                            column = row;
                            break;
                        case 1:
                            column = (gamePad.GetLength(0) - 1) - row;
                            break;
                    }

                    lineMatched &= (cellXOSymbol == (XOSymbol)gamePad.GetValue(row, column));
                    if (!lineMatched)
                        break;
                }
                if (lineMatched)
                    endGame(cellXOSymbol);
            }
        }

        /// <summary>
        /// Check if the game is over with no winner , because the Game Pad if full
        /// Sample Data : 
        /// X O X 
        /// X O O 
        /// O X O
        /// </summary>
        private void checkGameOver()
        {
            bool noEmptyCells = true; //Assume there is no empty cells
            for (int row = 0; row < gamePad.GetLength(0); row++)
            {
                for (int column = 0; column < gamePad.GetLength(1); column++)
                {
                    noEmptyCells &= ((XOSymbol)gamePad.GetValue(row, column) != XOSymbol.EMPTY);
                }
            }
            if (noEmptyCells)
                endGame(XOSymbol.EMPTY);
        }

        /// <summary>
        /// 1- Set the gameover flag
        /// 2- Find the winner
        /// </summary>
        /// <param name="winnerXOSymbol"></param>
        private void endGame( XOSymbol winnerXOSymbol)
        {
            isGameOver = true;
            if (humanPlayer.XOSymbol == winnerXOSymbol)
            {
                winnerPlayer = humanPlayer;
                return;
            }
            if (pcPlayer.XOSymbol == winnerXOSymbol)
            {
                winnerPlayer = pcPlayer;
                return;
            }
        }
    }
}
