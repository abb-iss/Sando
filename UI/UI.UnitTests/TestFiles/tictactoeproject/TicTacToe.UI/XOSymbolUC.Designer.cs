namespace TicTacToe.UI
{
    partial class XOSymbolUC
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
            this.pbxXOSymbol = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxXOSymbol)).BeginInit();
            this.SuspendLayout();
            // 
            // pbxXOSymbol
            // 
            this.pbxXOSymbol.Location = new System.Drawing.Point(1, 1);
            this.pbxXOSymbol.Name = "pbxXOSymbol";
            this.pbxXOSymbol.Size = new System.Drawing.Size(39, 39);
            this.pbxXOSymbol.TabIndex = 0;
            this.pbxXOSymbol.TabStop = false;
            // 
            // XOSymbolUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbxXOSymbol);
            this.Name = "XOSymbolUC";
            this.Size = new System.Drawing.Size(40, 40);
            ((System.ComponentModel.ISupportInitialize)(this.pbxXOSymbol)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbxXOSymbol;
    }
}
