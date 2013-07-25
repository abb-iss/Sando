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
    public partial class XOSymbolUC : UserControl
    {
        private XOSymbol xoSymbol;

        public XOSymbolUC()
        {
            InitializeComponent();
        }

        public XOSymbolUC(XOSymbol xoSymbol)
            : this()
        {
            XOSymbol = xoSymbol;
        }

        public XOSymbol XOSymbol
        {
            get { return xoSymbol; }
            set 
            { 
                xoSymbol = value;
                switch (xoSymbol)
                {
                    case XOSymbol.O:
                        this.pbxXOSymbol.BackgroundImage = rexImages.O;
                        break;
                    case XOSymbol.X:
                        this.pbxXOSymbol.BackgroundImage = rexImages.X;
                        break;
                }
            }
        }


    }
}
