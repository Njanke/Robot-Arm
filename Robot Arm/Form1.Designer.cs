namespace Robot_Arm
{
    partial class ShapeSorter
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
            this.capturePictureBox = new System.Windows.Forms.PictureBox();
            this.processPictureBox = new System.Windows.Forms.PictureBox();
            this.workspacePictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SORT = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.capturePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.processPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspacePictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // capturePictureBox
            // 
            this.capturePictureBox.BackColor = System.Drawing.SystemColors.Control;
            this.capturePictureBox.Location = new System.Drawing.Point(12, 12);
            this.capturePictureBox.Name = "capturePictureBox";
            this.capturePictureBox.Size = new System.Drawing.Size(320, 240);
            this.capturePictureBox.TabIndex = 2;
            this.capturePictureBox.TabStop = false;
            // 
            // processPictureBox
            // 
            this.processPictureBox.BackColor = System.Drawing.SystemColors.Control;
            this.processPictureBox.Location = new System.Drawing.Point(12, 258);
            this.processPictureBox.Name = "processPictureBox";
            this.processPictureBox.Size = new System.Drawing.Size(320, 240);
            this.processPictureBox.TabIndex = 3;
            this.processPictureBox.TabStop = false;
            // 
            // workspacePictureBox
            // 
            this.workspacePictureBox.BackColor = System.Drawing.SystemColors.Control;
            this.workspacePictureBox.Location = new System.Drawing.Point(338, 12);
            this.workspacePictureBox.Name = "workspacePictureBox";
            this.workspacePictureBox.Size = new System.Drawing.Size(550, 425);
            this.workspacePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.workspacePictureBox.TabIndex = 4;
            this.workspacePictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(339, 444);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "1st Shape Coord:";
            // 
            // SORT
            // 
            this.SORT.Location = new System.Drawing.Point(895, 13);
            this.SORT.Name = "SORT";
            this.SORT.Size = new System.Drawing.Size(247, 140);
            this.SORT.TabIndex = 6;
            this.SORT.Text = "SORT";
            this.SORT.UseVisualStyleBackColor = true;
            this.SORT.Click += new System.EventHandler(this.SORT_Click);
            // 
            // ShapeSorter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1154, 513);
            this.Controls.Add(this.SORT);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.workspacePictureBox);
            this.Controls.Add(this.processPictureBox);
            this.Controls.Add(this.capturePictureBox);
            this.Name = "ShapeSorter";
            this.Text = "Shape Sorter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShapeSorter_FormClosing);
            this.Load += new System.EventHandler(this.ShapeSorter_Load);
            ((System.ComponentModel.ISupportInitialize)(this.capturePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.processPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspacePictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NumericUpDown guy;
        private System.Windows.Forms.PictureBox capturePictureBox;
        private System.Windows.Forms.PictureBox processPictureBox;
        private System.Windows.Forms.PictureBox workspacePictureBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SORT;
    }
}

