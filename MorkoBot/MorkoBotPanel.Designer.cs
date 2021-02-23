namespace MorkoBot
{
    partial class MorkoBotPanel
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
            this.ChatBox = new System.Windows.Forms.TextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ChatBox
            // 
            this.ChatBox.Location = new System.Drawing.Point(12, 16);
            this.ChatBox.Name = "ChatBox";
            this.ChatBox.Size = new System.Drawing.Size(150, 20);
            this.ChatBox.TabIndex = 2;
            // 
            // SendButton
            // 
            this.SendButton.Location = new System.Drawing.Point(168, 12);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(141, 27);
            this.SendButton.TabIndex = 3;
            this.SendButton.Text = "Send Message";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // MorkoBotPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 51);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.ChatBox);
            this.Name = "MorkoBotPanel";
            this.Text = "MorkoBot";
            this.Load += new System.EventHandler(this.MorkoBotPanel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ChatBox;
        private System.Windows.Forms.Button SendButton;
    }
}

