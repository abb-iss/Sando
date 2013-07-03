using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TicTacToe.Classes;

namespace TicTacToe.UI
{
    public partial class GamePadUC : UserControl
    {
        private GameEngine gameEngine;
        public event GameEventHandler GameFinished;
        public event GameEventHandler MovePlayed;

        public delegate void GameEventHandler(GameEngine gameEngine);

        public GamePadUC()
        {
            InitializeComponent();
        }

        protected void pnlGamePad_MouseClick(object sender, MouseEventArgs e)
        {
            int cellSize = pnlGamePad.Height / 3;
            int row = e.Y / cellSize;
            int column = e.X / cellSize;

            if (pnlGamePad.GetControlFromPosition(column, row) != null)
                return;

            int position = (row * 3) + column;
            GameMove gameMove = new GameMove(position, gameEngine.HumanPlayer.XOSymbol);
            AddMove(gameMove);
            gameEngine.HumanPlayer.GameMove = gameMove;
            gameEngine.HumanPlayer.Play();
            if (gameEngine.IsGameOver && (gameEngine.WinnerPlayer == gameEngine.HumanPlayer))
            {
                GameFinished(gameEngine);
                return;
            }
            AddMove(gameEngine.PCPlayer.GameMove);
            if (gameEngine.IsGameOver)
            {
                GameFinished(gameEngine);
                return;
            }
            MovePlayed(gameEngine);
        }

        public void AddMove(GameMove gameMove)
        {
            if (pnlGamePad.GetControlFromPosition(gameMove.ColumnIndex, gameMove.RowIndex) != null && gameEngine != null)
                return;

            XOSymbolUC xoSymbolUC = new XOSymbolUC(gameMove.XOSymbol);
            this.pnlGamePad.Controls.Add(xoSymbolUC, gameMove.ColumnIndex, gameMove.RowIndex);
        }

        public GameEngine GameEngine
        {
            get { return gameEngine; }
            set
            {
                gameEngine = value;
                reset();
                if (value != null)
                {
                    showGamePad();
                }
            }
        }

        protected XOSymbol[,] GamePad
        {
            set
            {
                gameEngine = null;
                showGamePad(value);
            }
        }

        private void reset()
        {
            pnlGamePad.Controls.Clear();
        }

        private void showGamePad()
        {
            showGamePad(gameEngine.GamePad);
        }

        private void showGamePad(XOSymbol[,] gamePad)
        {
            int position = 0;
            for (int row = 0; row < gamePad.GetLength(0); row++)
            {
                for (int column = 0; column < gamePad.GetLength(1); column++)
                {
                    XOSymbol xoSymbol = (XOSymbol)gamePad.GetValue(row, column);
                    if (xoSymbol != XOSymbol.EMPTY)
                        AddMove(new GameMove(position, xoSymbol));
                    position++;
                }
            }
        }
    }
}
