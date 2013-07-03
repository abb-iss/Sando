using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TicTacToe.Classes;

namespace TicTacToe.UI.GameTracing
{
    public partial class TracingGamePadUC : TicTacToe.UI.GamePadUC
    {

        public TracingGamePadUC()
        {
            InitializeComponent();
            this.Controls[6].Enabled = false;
        }

        public TracingGamePadUC(GamePadNode gamePadNode)
            : this()
        {
            SetGamePad(gamePadNode);
        }

        public void SetGamePad(GamePadNode gamePadNode)
        {
            this.GamePad = gamePadNode.GamePad;
            lblHeuristicValue.Text = gamePadNode.Heurisitic + "";
            lblFOValue.Text = gamePadNode.OFunction + "";
            lblFXValue.Text = gamePadNode.XFunction + "";
        }

    }
}
