namespace TicTacToe.UI.GameTracing
{
    partial class TracingForm
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
            this.pnlTracing = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // pnlTracing
            // 
            this.pnlTracing.AutoSize = true;
            this.pnlTracing.ColumnCount = 1;
            this.pnlTracing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlTracing.Location = new System.Drawing.Point(12, 12);
            this.pnlTracing.Name = "pnlTracing";
            this.pnlTracing.RowCount = 1;
            this.pnlTracing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 181F));
            this.pnlTracing.Size = new System.Drawing.Size(1351, 181);
            this.pnlTracing.TabIndex = 0;
            // 
            // TracingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(784, 264);
            this.Controls.Add(this.pnlTracing);
            this.Name = "TracingForm";
            this.Text = "TracingForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlTracing;
    }
}