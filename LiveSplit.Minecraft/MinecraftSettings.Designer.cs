namespace LiveSplit.Minecraft
{
    partial class MinecraftSettings
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
            this.labelVersion = new System.Windows.Forms.Label();
            this.linkInstructions = new System.Windows.Forms.LinkLabel();
            this.checkBoxAutosplitter = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(10, 463);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(114, 13);
            this.labelVersion.TabIndex = 6;
            this.labelVersion.Text = "Version ?.?.? by Kohru";
            // 
            // linkInstructions
            // 
            this.linkInstructions.AutoSize = true;
            this.linkInstructions.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.linkInstructions.Location = new System.Drawing.Point(33, 261);
            this.linkInstructions.Name = "linkInstructions";
            this.linkInstructions.Size = new System.Drawing.Size(396, 24);
            this.linkInstructions.TabIndex = 11;
            this.linkInstructions.TabStop = true;
            this.linkInstructions.Text = "INSTRUCTIONS AND FAQ, PLEASE WATCH";
            this.linkInstructions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkInstructions_LinkClicked);
            // 
            // checkBoxAutosplitter
            // 
            this.checkBoxAutosplitter.AutoSize = true;
            this.checkBoxAutosplitter.Location = new System.Drawing.Point(10, 0);
            this.checkBoxAutosplitter.Name = "checkBoxAutosplitter";
            this.checkBoxAutosplitter.Size = new System.Drawing.Size(149, 17);
            this.checkBoxAutosplitter.TabIndex = 12;
            this.checkBoxAutosplitter.Text = "Enable autosplitter (+1.12)";
            this.checkBoxAutosplitter.UseVisualStyleBackColor = true;
            this.checkBoxAutosplitter.CheckedChanged += new System.EventHandler(this.CheckBoxAutosplitter_CheckedChanged);
            // 
            // MinecraftSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBoxAutosplitter);
            this.Controls.Add(this.linkInstructions);
            this.Controls.Add(this.labelVersion);
            this.Name = "MinecraftSettings";
            this.Size = new System.Drawing.Size(475, 485);
            this.Load += new System.EventHandler(this.MinecraftSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.LinkLabel linkInstructions;
        private System.Windows.Forms.CheckBox checkBoxAutosplitter;
    }
}
