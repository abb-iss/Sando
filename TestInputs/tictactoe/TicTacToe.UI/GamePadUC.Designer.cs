namespace TicTacToe.UI
{
    partial class GamePadUC
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlGamePad = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // pnlGamePad
            // 
            this.pnlGamePad.BackColor = System.Drawing.Color.Transparent;
            this.pnlGamePad.ColumnCount = 3;
            this.pnlGamePad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlGamePad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlGamePad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlGamePad.Location = new System.Drawing.Point(1, 1);
            this.pnlGamePad.Name = "pnlGamePad";
            this.pnlGamePad.RowCount = 3;
            this.pnlGamePad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlGamePad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlGamePad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlGamePad.Size = new System.Drawing.Size(135, 135);
            this.pnlGamePad.TabIndex = 0;
            this.pnlGamePad.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlGamePad_MouseClick);
            // 
            // GamePadUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::TicTacToe.UI.rexImages.GamePad;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.pnlGamePad);
            this.DoubleBuffered = true;
            this.Name = "GamePadUC";
            this.Size = new System.Drawing.Size(136, 136);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlGamePad;
    }
}
