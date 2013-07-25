namespace TicTacToe.UI.GameTracing
{
    partial class TracingGamePadUC
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblHeuristic = new System.Windows.Forms.Label();
            this.lblHeuristicValue = new System.Windows.Forms.Label();
            this.lblFX = new System.Windows.Forms.Label();
            this.lblFO = new System.Windows.Forms.Label();
            this.lblFXValue = new System.Windows.Forms.Label();
            this.lblFOValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblHeuristic
            // 
            this.lblHeuristic.AutoSize = true;
            this.lblHeuristic.Location = new System.Drawing.Point(5, 141);
            this.lblHeuristic.Name = "lblHeuristic";
            this.lblHeuristic.Size = new System.Drawing.Size(51, 13);
            this.lblHeuristic.TabIndex = 1;
            this.lblHeuristic.Text = "Heuristic:";
            // 
            // lblHeuristicValue
            // 
            this.lblHeuristicValue.AutoSize = true;
            this.lblHeuristicValue.Location = new System.Drawing.Point(55, 141);
            this.lblHeuristicValue.Name = "lblHeuristicValue";
            this.lblHeuristicValue.Size = new System.Drawing.Size(0, 13);
            this.lblHeuristicValue.TabIndex = 2;
            // 
            // lblFX
            // 
            this.lblFX.AutoSize = true;
            this.lblFX.Location = new System.Drawing.Point(6, 158);
            this.lblFX.Name = "lblFX";
            this.lblFX.Size = new System.Drawing.Size(29, 13);
            this.lblFX.TabIndex = 3;
            this.lblFX.Text = "F(X):";
            // 
            // lblFO
            // 
            this.lblFO.AutoSize = true;
            this.lblFO.Location = new System.Drawing.Point(56, 158);
            this.lblFO.Name = "lblFO";
            this.lblFO.Size = new System.Drawing.Size(30, 13);
            this.lblFO.TabIndex = 4;
            this.lblFO.Text = "F(O):";
            // 
            // lblFXValue
            // 
            this.lblFXValue.AutoSize = true;
            this.lblFXValue.Location = new System.Drawing.Point(40, 158);
            this.lblFXValue.Name = "lblFXValue";
            this.lblFXValue.Size = new System.Drawing.Size(0, 13);
            this.lblFXValue.TabIndex = 5;
            // 
            // lblFOValue
            // 
            this.lblFOValue.AutoSize = true;
            this.lblFOValue.Location = new System.Drawing.Point(90, 158);
            this.lblFOValue.Name = "lblFOValue";
            this.lblFOValue.Size = new System.Drawing.Size(0, 13);
            this.lblFOValue.TabIndex = 6;
            // 
            // TracingGamePadUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.lblFOValue);
            this.Controls.Add(this.lblFXValue);
            this.Controls.Add(this.lblFO);
            this.Controls.Add(this.lblFX);
            this.Controls.Add(this.lblHeuristicValue);
            this.Controls.Add(this.lblHeuristic);
            this.Name = "TracingGamePadUC";
            this.Size = new System.Drawing.Size(136, 175);
            this.Controls.SetChildIndex(this.lblHeuristic, 0);
            this.Controls.SetChildIndex(this.lblHeuristicValue, 0);
            this.Controls.SetChildIndex(this.lblFX, 0);
            this.Controls.SetChildIndex(this.lblFO, 0);
            this.Controls.SetChildIndex(this.lblFXValue, 0);
            this.Controls.SetChildIndex(this.lblFOValue, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHeuristic;
        private System.Windows.Forms.Label lblHeuristicValue;
        private System.Windows.Forms.Label lblFX;
        private System.Windows.Forms.Label lblFO;
        private System.Windows.Forms.Label lblFXValue;
        private System.Windows.Forms.Label lblFOValue;
    }
}
