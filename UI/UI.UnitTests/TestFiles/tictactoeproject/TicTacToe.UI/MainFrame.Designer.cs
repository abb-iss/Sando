namespace TicTacToe.UI
{
    partial class MainFrame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrame));
            this.grbGameSettings = new System.Windows.Forms.GroupBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.rbtO = new System.Windows.Forms.RadioButton();
            this.rbtX = new System.Windows.Forms.RadioButton();
            this.lblSelectYourSymbol = new System.Windows.Forms.Label();
            this.lblFirstPlayer = new System.Windows.Forms.Label();
            this.pnlYesNo = new System.Windows.Forms.Panel();
            this.rbtNo = new System.Windows.Forms.RadioButton();
            this.rbtYes = new System.Windows.Forms.RadioButton();
            this.cbxTracing = new System.Windows.Forms.CheckBox();
            this.gamePadUC1 = new TicTacToe.UI.GamePadUC();
            this.grbGameSettings.SuspendLayout();
            this.pnlYesNo.SuspendLayout();
            this.SuspendLayout();
            // 
            // grbGameSettings
            // 
            this.grbGameSettings.Controls.Add(this.btnStart);
            this.grbGameSettings.Controls.Add(this.rbtO);
            this.grbGameSettings.Controls.Add(this.rbtX);
            this.grbGameSettings.Controls.Add(this.lblSelectYourSymbol);
            this.grbGameSettings.Controls.Add(this.lblFirstPlayer);
            this.grbGameSettings.Controls.Add(this.pnlYesNo);
            this.grbGameSettings.Location = new System.Drawing.Point(12, 164);
            this.grbGameSettings.Name = "grbGameSettings";
            this.grbGameSettings.Size = new System.Drawing.Size(260, 100);
            this.grbGameSettings.TabIndex = 1;
            this.grbGameSettings.TabStop = false;
            this.grbGameSettings.Text = "Game Settings";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(179, 71);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // rbtO
            // 
            this.rbtO.AutoSize = true;
            this.rbtO.Location = new System.Drawing.Point(143, 49);
            this.rbtO.Name = "rbtO";
            this.rbtO.Size = new System.Drawing.Size(33, 17);
            this.rbtO.TabIndex = 5;
            this.rbtO.TabStop = true;
            this.rbtO.Text = "O";
            this.rbtO.UseVisualStyleBackColor = true;
            // 
            // rbtX
            // 
            this.rbtX.AutoSize = true;
            this.rbtX.Location = new System.Drawing.Point(107, 49);
            this.rbtX.Name = "rbtX";
            this.rbtX.Size = new System.Drawing.Size(32, 17);
            this.rbtX.TabIndex = 4;
            this.rbtX.TabStop = true;
            this.rbtX.Text = "X";
            this.rbtX.UseVisualStyleBackColor = true;
            // 
            // lblSelectYourSymbol
            // 
            this.lblSelectYourSymbol.AutoSize = true;
            this.lblSelectYourSymbol.Location = new System.Drawing.Point(7, 51);
            this.lblSelectYourSymbol.Name = "lblSelectYourSymbol";
            this.lblSelectYourSymbol.Size = new System.Drawing.Size(95, 13);
            this.lblSelectYourSymbol.TabIndex = 1;
            this.lblSelectYourSymbol.Text = "Select your symbol";
            // 
            // lblFirstPlayer
            // 
            this.lblFirstPlayer.AutoSize = true;
            this.lblFirstPlayer.Location = new System.Drawing.Point(7, 26);
            this.lblFirstPlayer.Name = "lblFirstPlayer";
            this.lblFirstPlayer.Size = new System.Drawing.Size(108, 13);
            this.lblFirstPlayer.TabIndex = 0;
            this.lblFirstPlayer.Text = "Do you want to start?";
            // 
            // pnlYesNo
            // 
            this.pnlYesNo.Controls.Add(this.rbtNo);
            this.pnlYesNo.Controls.Add(this.rbtYes);
            this.pnlYesNo.Location = new System.Drawing.Point(117, 19);
            this.pnlYesNo.Name = "pnlYesNo";
            this.pnlYesNo.Size = new System.Drawing.Size(92, 26);
            this.pnlYesNo.TabIndex = 7;
            // 
            // rbtNo
            // 
            this.rbtNo.AutoSize = true;
            this.rbtNo.Location = new System.Drawing.Point(49, 5);
            this.rbtNo.Name = "rbtNo";
            this.rbtNo.Size = new System.Drawing.Size(39, 17);
            this.rbtNo.TabIndex = 3;
            this.rbtNo.Text = "No";
            this.rbtNo.UseVisualStyleBackColor = true;
            // 
            // rbtYes
            // 
            this.rbtYes.AutoSize = true;
            this.rbtYes.Checked = true;
            this.rbtYes.Location = new System.Drawing.Point(4, 5);
            this.rbtYes.Name = "rbtYes";
            this.rbtYes.Size = new System.Drawing.Size(43, 17);
            this.rbtYes.TabIndex = 2;
            this.rbtYes.TabStop = true;
            this.rbtYes.Text = "Yes";
            this.rbtYes.UseVisualStyleBackColor = true;
            // 
            // cbxTracing
            // 
            this.cbxTracing.AutoSize = true;
            this.cbxTracing.Location = new System.Drawing.Point(12, 269);
            this.cbxTracing.Name = "cbxTracing";
            this.cbxTracing.Size = new System.Drawing.Size(95, 17);
            this.cbxTracing.TabIndex = 8;
            this.cbxTracing.Text = "Display tracing";
            this.cbxTracing.UseVisualStyleBackColor = true;
            this.cbxTracing.CheckedChanged += new System.EventHandler(this.cbxTracing_CheckedChanged);
            // 
            // gamePadUC1
            // 
            this.gamePadUC1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gamePadUC1.BackgroundImage")));
            this.gamePadUC1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.gamePadUC1.Enabled = false;
            this.gamePadUC1.GameEngine = null;
            this.gamePadUC1.Location = new System.Drawing.Point(74, 16);
            this.gamePadUC1.Name = "gamePadUC1";
            this.gamePadUC1.Size = new System.Drawing.Size(136, 136);
            this.gamePadUC1.TabIndex = 0;
            this.gamePadUC1.GameFinished += new TicTacToe.UI.GamePadUC.GameEventHandler(this.gamePadUC1_GameFinished);
            this.gamePadUC1.MovePlayed += new TicTacToe.UI.GamePadUC.GameEventHandler(this.gamePadUC1_MovePlayed);
            // 
            // MainFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 290);
            this.Controls.Add(this.cbxTracing);
            this.Controls.Add(this.grbGameSettings);
            this.Controls.Add(this.gamePadUC1);
            this.Name = "MainFrame";
            this.Text = "Tic Tac Toe";
            this.grbGameSettings.ResumeLayout(false);
            this.grbGameSettings.PerformLayout();
            this.pnlYesNo.ResumeLayout(false);
            this.pnlYesNo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GamePadUC gamePadUC1;
        private System.Windows.Forms.GroupBox grbGameSettings;
        private System.Windows.Forms.RadioButton rbtYes;
        private System.Windows.Forms.Label lblSelectYourSymbol;
        private System.Windows.Forms.Label lblFirstPlayer;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.RadioButton rbtO;
        private System.Windows.Forms.RadioButton rbtX;
        private System.Windows.Forms.RadioButton rbtNo;
        private System.Windows.Forms.Panel pnlYesNo;
        private System.Windows.Forms.CheckBox cbxTracing;



    }
}

