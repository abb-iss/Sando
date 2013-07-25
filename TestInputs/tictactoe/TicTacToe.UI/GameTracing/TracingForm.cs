using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TicTacToe.Classes;

namespace TicTacToe.UI.GameTracing
{
    public partial class TracingForm : Form
    {
        public TracingForm()
        {
            InitializeComponent();
        }

        public void AddTracingLevel(List<GamePadNode> listGamePadNode)
        {
            SingleLevelUC singleLevelUC = new SingleLevelUC();
            foreach (GamePadNode node in listGamePadNode)
            {
                singleLevelUC.AddGamePad(node);
            }
            this.pnlTracing.Controls.Add(singleLevelUC);
            this.VerticalScroll.Value = this.VerticalScroll.Maximum;
            this.VerticalScroll.Value = this.VerticalScroll.Maximum;
        }

        public void Reset()
        {
            this.pnlTracing.Controls.Clear();
        }
    }
}
