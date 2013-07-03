using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TicTacToe.Classes;

namespace TicTacToe.UI
{
    public partial class MainFrame : Form
    {
        private TicTacToe.UI.GameTracing.TracingForm tracingForm = new TicTacToe.UI.GameTracing.TracingForm();

        public MainFrame()
        {
            InitializeComponent();
            tracingForm.Visible = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            HumanPlayer humanPlayer = new HumanPlayer("Moutasem Al-awa", getSelectedSymbol());
            XOSymbol xoSymbolPCPlayer = XOSymbol.EMPTY;
            
            switch(getSelectedSymbol())
            {
                case XOSymbol.O:
                    xoSymbolPCPlayer = XOSymbol.X;
                    break;
                case XOSymbol.X:
                    xoSymbolPCPlayer = XOSymbol.O;
                    break;
            }
            PCPlayer pcPlayer = new PCPlayer(xoSymbolPCPlayer);

            Player firstPlayer = null;
            firstPlayer = pcPlayer;
            if (isHumanFirst())
                firstPlayer = humanPlayer;

            GameEngine gameEngine = new GameEngine(humanPlayer, pcPlayer, firstPlayer);
            gamePadUC1.GameEngine = gameEngine;

            grbGameSettings.Enabled = false;
            gamePadUC1.Enabled = true;

            if (!isHumanFirst())
            {
                pcPlayer.Play();
                gamePadUC1.AddMove(pcPlayer.GameMove);
            }
        }

        private XOSymbol getSelectedSymbol()
        {
            if (rbtX.Checked)
                return XOSymbol.X;
            if (rbtO.Checked)
                return XOSymbol.O;
            return XOSymbol.EMPTY;
        }

        private bool isHumanFirst()
        {
            return rbtYes.Checked;
        }

        private void gamePadUC1_GameFinished(GameEngine gameEngine)
        {
            showWinner(gameEngine.WinnerPlayer);
            btnStart_Click(null, null);
            tracingForm.Reset();
        }

        private void showWinner(Player player)
        {
            if (player == null)
            {
                MessageBox.Show("Game finished with no winner");
                return;
            }
            if (player is HumanPlayer)
            {
                MessageBox.Show("You win :)");
                return;
            }
            if (player is PCPlayer)
            {
                MessageBox.Show("You lose :(");
                return;
            }
        }

        private void cbxTracing_CheckedChanged(object sender, EventArgs e)
        {
            tracingForm.Visible = cbxTracing.Checked;
            tracingForm.Top = this.Top + this.Height + 10;
            tracingForm.Left = this.Left;
        }

        private void gamePadUC1_MovePlayed(GameEngine gameEngine)
        {
            tracingForm.AddTracingLevel(gameEngine.PCPlayer.IPlayerBrain.GetGamePadNodes());
        }
 
    }
}
