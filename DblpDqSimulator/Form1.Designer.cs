namespace DblpDqSimulator
{
    partial class frmGen
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
            this.btnRun = new System.Windows.Forms.Button();
            this.btnRubHierarchial = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(12, 12);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(155, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "&Run Single Histogram";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnRubHierarchial
            // 
            this.btnRubHierarchial.Location = new System.Drawing.Point(12, 70);
            this.btnRubHierarchial.Name = "btnRubHierarchial";
            this.btnRubHierarchial.Size = new System.Drawing.Size(155, 23);
            this.btnRubHierarchial.TabIndex = 1;
            this.btnRubHierarchial.Text = "&Run Hierarchial Histogram";
            this.btnRubHierarchial.UseVisualStyleBackColor = true;
            this.btnRubHierarchial.Click += new System.EventHandler(this.btnRubHierarchial_Click);
            // 
            // frmGen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(180, 111);
            this.Controls.Add(this.btnRubHierarchial);
            this.Controls.Add(this.btnRun);
            this.Name = "frmGen";
            this.Text = "Generate DQ metric Simulated Loockup";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnRubHierarchial;
    }
}

