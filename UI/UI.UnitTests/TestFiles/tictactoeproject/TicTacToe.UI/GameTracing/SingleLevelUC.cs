using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TicTacToe.Classes;

namespace TicTacToe.UI.GameTracing
{
    public partial class SingleLevelUC : UserControl
    {
        public SingleLevelUC()
        {
            InitializeComponent();
        }

        public void AddGamePad(GamePadNode gamePadNode)
        {
            pnlGamePads.Controls.Add(new TracingGamePadUC(gamePadNode));
        }

        private void pnlGamePads_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
