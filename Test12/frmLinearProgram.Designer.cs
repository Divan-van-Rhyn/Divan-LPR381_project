using System;

namespace Test12
{
    partial class frmLinearSolver
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
            this.btnKnapsack = new System.Windows.Forms.Button();
            this.redtInput = new System.Windows.Forms.RichTextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnLoadFrom = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.dgvOptimal = new System.Windows.Forms.DataGridView();
            this.lblOptimal = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.btnGraphical = new System.Windows.Forms.Button();
            this.btnSimplex = new System.Windows.Forms.Button();
            this.btnSaveTo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOptimal)).BeginInit();
            this.SuspendLayout();
            // 
            // btnKnapsack
            // 
            this.btnKnapsack.Location = new System.Drawing.Point(270, 602);
            this.btnKnapsack.Name = "btnKnapsack";
            this.btnKnapsack.Size = new System.Drawing.Size(84, 23);
            this.btnKnapsack.TabIndex = 0;
            this.btnKnapsack.Text = "Knapsack";
            this.btnKnapsack.UseVisualStyleBackColor = true;
            this.btnKnapsack.Click += new System.EventHandler(this.btnKnapsack_Click);
            // 
            // redtInput
            // 
            this.redtInput.Location = new System.Drawing.Point(124, 395);
            this.redtInput.Name = "redtInput";
            this.redtInput.Size = new System.Drawing.Size(338, 189);
            this.redtInput.TabIndex = 1;
            this.redtInput.Text = "Insert canonical form or load from text file";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(810, 602);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(85, 23);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnLoadFrom
            // 
            this.btnLoadFrom.Location = new System.Drawing.Point(16, 396);
            this.btnLoadFrom.Name = "btnLoadFrom";
            this.btnLoadFrom.Size = new System.Drawing.Size(102, 23);
            this.btnLoadFrom.TabIndex = 3;
            this.btnLoadFrom.Text = "Load from File";
            this.btnLoadFrom.UseVisualStyleBackColor = true;
            this.btnLoadFrom.Click += new System.EventHandler(this.btnTextfile_Click);
            // 
            // dgvOptimal
            // 
            this.dgvOptimal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOptimal.Location = new System.Drawing.Point(558, 395);
            this.dgvOptimal.Margin = new System.Windows.Forms.Padding(2);
            this.dgvOptimal.Name = "dgvOptimal";
            this.dgvOptimal.RowHeadersWidth = 51;
            this.dgvOptimal.RowTemplate.Height = 24;
            this.dgvOptimal.Size = new System.Drawing.Size(337, 189);
            this.dgvOptimal.TabIndex = 5;
            // 
            // lblOptimal
            // 
            this.lblOptimal.AutoSize = true;
            this.lblOptimal.Location = new System.Drawing.Point(556, 379);
            this.lblOptimal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOptimal.Name = "lblOptimal";
            this.lblOptimal.Size = new System.Drawing.Size(75, 13);
            this.lblOptimal.TabIndex = 6;
            this.lblOptimal.Text = "Optimal Value:";
            this.lblOptimal.Click += new System.EventHandler(this.lblOptimalValue_Click_1);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 668);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1080, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // btnGraphical
            // 
            this.btnGraphical.Location = new System.Drawing.Point(162, 602);
            this.btnGraphical.Margin = new System.Windows.Forms.Padding(2);
            this.btnGraphical.Name = "btnGraphical";
            this.btnGraphical.Size = new System.Drawing.Size(85, 23);
            this.btnGraphical.TabIndex = 11;
            this.btnGraphical.Text = "Graphical";
            this.btnGraphical.UseVisualStyleBackColor = true;
            this.btnGraphical.Click += new System.EventHandler(this.btnGraphical_Click);
            // 
            // btnSimplex
            // 
            this.btnSimplex.Location = new System.Drawing.Point(378, 601);
            this.btnSimplex.Margin = new System.Windows.Forms.Padding(2);
            this.btnSimplex.Name = "btnSimplex";
            this.btnSimplex.Size = new System.Drawing.Size(84, 24);
            this.btnSimplex.TabIndex = 12;
            this.btnSimplex.Text = "Simplex";
            this.btnSimplex.UseVisualStyleBackColor = true;
            this.btnSimplex.Click += new System.EventHandler(this.btnSimplex_Click);
            // 
            // btnSaveTo
            // 
            this.btnSaveTo.Location = new System.Drawing.Point(17, 433);
            this.btnSaveTo.Margin = new System.Windows.Forms.Padding(2);
            this.btnSaveTo.Name = "btnSaveTo";
            this.btnSaveTo.Size = new System.Drawing.Size(102, 24);
            this.btnSaveTo.TabIndex = 13;
            this.btnSaveTo.Text = "Save to file";
            this.btnSaveTo.UseVisualStyleBackColor = true;
            this.btnSaveTo.Click += new System.EventHandler(this.btnSaveTo_Click);
            // 
            // frmLinearSolver
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1080, 690);
            this.Controls.Add(this.btnSaveTo);
            this.Controls.Add(this.btnSimplex);
            this.Controls.Add(this.btnGraphical);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lblOptimal);
            this.Controls.Add(this.dgvOptimal);
            this.Controls.Add(this.btnLoadFrom);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.redtInput);
            this.Controls.Add(this.btnKnapsack);
            this.Name = "frmLinearSolver";
            this.Text = "Linear Programming Solver";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOptimal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.Button btnKnapsack;
        private System.Windows.Forms.RichTextBox redtInput;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnLoadFrom;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.DataGridView dgvOptimal;
        private System.Windows.Forms.Label lblOptimal;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button btnGraphical;
        private System.Windows.Forms.Button btnSimplex;
        private System.Windows.Forms.Button btnSaveTo;
    }
}
