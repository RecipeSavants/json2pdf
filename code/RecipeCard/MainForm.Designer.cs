namespace RecipeCard
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.iTalk_ThemeContainer1 = new iTalk.iTalk_ThemeContainer();
            this.btnId = new iTalk.iTalk_Button_1();
            this.groupBoxPDF = new System.Windows.Forms.GroupBox();
            this.btnStartPDF = new iTalk.iTalk_Button_1();
            this.txtJSONPDF = new iTalk.iTalk_RichTextBox();
            this.lblJSONPDF = new iTalk.iTalk_Label();
            this.btnJSONPDF = new iTalk.iTalk_Button_1();
            this.txtID = new iTalk.iTalk_RichTextBox();
            this.lblD = new iTalk.iTalk_Label();
            this.txtJSON = new iTalk.iTalk_RichTextBox();
            this.lblJSON = new iTalk.iTalk_Label();
            this.btnJSON = new iTalk.iTalk_Button_1();
            this.txtOutput = new iTalk.iTalk_RichTextBox();
            this.btnStart = new iTalk.iTalk_Button_1();
            this.iTalk_ControlBox1 = new iTalk.iTalk_ControlBox();
            this.txtCfg = new iTalk.iTalk_RichTextBox();
            this.lblCfg = new iTalk.iTalk_Label();
            this.btnCfg = new iTalk.iTalk_Button_1();
            this.iTalk_ThemeContainer1.SuspendLayout();
            this.groupBoxPDF.SuspendLayout();
            this.SuspendLayout();
            // 
            // iTalk_ThemeContainer1
            // 
            this.iTalk_ThemeContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.iTalk_ThemeContainer1.Controls.Add(this.txtCfg);
            this.iTalk_ThemeContainer1.Controls.Add(this.lblCfg);
            this.iTalk_ThemeContainer1.Controls.Add(this.btnCfg);
            this.iTalk_ThemeContainer1.Controls.Add(this.btnId);
            this.iTalk_ThemeContainer1.Controls.Add(this.groupBoxPDF);
            this.iTalk_ThemeContainer1.Controls.Add(this.txtID);
            this.iTalk_ThemeContainer1.Controls.Add(this.lblD);
            this.iTalk_ThemeContainer1.Controls.Add(this.txtJSON);
            this.iTalk_ThemeContainer1.Controls.Add(this.lblJSON);
            this.iTalk_ThemeContainer1.Controls.Add(this.btnJSON);
            this.iTalk_ThemeContainer1.Controls.Add(this.txtOutput);
            this.iTalk_ThemeContainer1.Controls.Add(this.btnStart);
            this.iTalk_ThemeContainer1.Controls.Add(this.iTalk_ControlBox1);
            this.iTalk_ThemeContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iTalk_ThemeContainer1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.iTalk_ThemeContainer1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.iTalk_ThemeContainer1.Location = new System.Drawing.Point(0, 0);
            this.iTalk_ThemeContainer1.Name = "iTalk_ThemeContainer1";
            this.iTalk_ThemeContainer1.Padding = new System.Windows.Forms.Padding(3, 28, 3, 28);
            this.iTalk_ThemeContainer1.Sizable = true;
            this.iTalk_ThemeContainer1.Size = new System.Drawing.Size(550, 500);
            this.iTalk_ThemeContainer1.SmartBounds = false;
            this.iTalk_ThemeContainer1.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.iTalk_ThemeContainer1.TabIndex = 0;
            this.iTalk_ThemeContainer1.Text = "Recipe Card";
            // 
            // btnId
            // 
            this.btnId.BackColor = System.Drawing.Color.Transparent;
            this.btnId.Font = new System.Drawing.Font("Arial", 10F);
            this.btnId.Image = null;
            this.btnId.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnId.Location = new System.Drawing.Point(376, 65);
            this.btnId.Name = "btnId";
            this.btnId.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnId.Size = new System.Drawing.Size(38, 27);
            this.btnId.TabIndex = 89;
            this.btnId.Text = "Clear";
            this.btnId.TextAlignment = System.Drawing.StringAlignment.Center;
            this.btnId.Click += new System.EventHandler(this.btnId_Click);
            // 
            // groupBoxPDF
            // 
            this.groupBoxPDF.Controls.Add(this.btnStartPDF);
            this.groupBoxPDF.Controls.Add(this.txtJSONPDF);
            this.groupBoxPDF.Controls.Add(this.lblJSONPDF);
            this.groupBoxPDF.Controls.Add(this.btnJSONPDF);
            this.groupBoxPDF.Location = new System.Drawing.Point(6, 131);
            this.groupBoxPDF.Name = "groupBoxPDF";
            this.groupBoxPDF.Size = new System.Drawing.Size(538, 45);
            this.groupBoxPDF.TabIndex = 62;
            this.groupBoxPDF.TabStop = false;
            this.groupBoxPDF.Text = "PDF";
            // 
            // btnStartPDF
            // 
            this.btnStartPDF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStartPDF.BackColor = System.Drawing.Color.Transparent;
            this.btnStartPDF.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartPDF.Image = null;
            this.btnStartPDF.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStartPDF.Location = new System.Drawing.Point(414, 12);
            this.btnStartPDF.Name = "btnStartPDF";
            this.btnStartPDF.Size = new System.Drawing.Size(120, 27);
            this.btnStartPDF.TabIndex = 75;
            this.btnStartPDF.Text = "START PDF";
            this.btnStartPDF.TextAlignment = System.Drawing.StringAlignment.Center;
            this.btnStartPDF.Click += new System.EventHandler(this.btnStartPDF_Click);
            // 
            // txtJSONPDF
            // 
            this.txtJSONPDF.AutoWordSelection = false;
            this.txtJSONPDF.BackColor = System.Drawing.Color.Transparent;
            this.txtJSONPDF.Font = new System.Drawing.Font("Tahoma", 10F);
            this.txtJSONPDF.ForeColor = System.Drawing.Color.DimGray;
            this.txtJSONPDF.Location = new System.Drawing.Point(46, 12);
            this.txtJSONPDF.Name = "txtJSONPDF";
            this.txtJSONPDF.ReadOnly = true;
            this.txtJSONPDF.SelectionColor = System.Drawing.Color.DimGray;
            this.txtJSONPDF.Size = new System.Drawing.Size(318, 27);
            this.txtJSONPDF.TabIndex = 79;
            this.txtJSONPDF.WordWrap = false;
            // 
            // lblJSONPDF
            // 
            this.lblJSONPDF.AutoSize = true;
            this.lblJSONPDF.BackColor = System.Drawing.Color.Transparent;
            this.lblJSONPDF.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblJSONPDF.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.lblJSONPDF.Location = new System.Drawing.Point(6, 18);
            this.lblJSONPDF.Name = "lblJSONPDF";
            this.lblJSONPDF.Size = new System.Drawing.Size(34, 13);
            this.lblJSONPDF.TabIndex = 75;
            this.lblJSONPDF.Text = "JSON";
            // 
            // btnJSONPDF
            // 
            this.btnJSONPDF.BackColor = System.Drawing.Color.Transparent;
            this.btnJSONPDF.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnJSONPDF.Image = global::RecipeCard.Properties.Resources.folder;
            this.btnJSONPDF.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnJSONPDF.Location = new System.Drawing.Point(370, 12);
            this.btnJSONPDF.Name = "btnJSONPDF";
            this.btnJSONPDF.Size = new System.Drawing.Size(38, 27);
            this.btnJSONPDF.TabIndex = 76;
            this.btnJSONPDF.TextAlignment = System.Drawing.StringAlignment.Center;
            this.btnJSONPDF.Click += new System.EventHandler(this.btnJSONPDF_Click);
            // 
            // txtID
            // 
            this.txtID.AutoWordSelection = false;
            this.txtID.BackColor = System.Drawing.Color.Transparent;
            this.txtID.Font = new System.Drawing.Font("Tahoma", 10F);
            this.txtID.ForeColor = System.Drawing.Color.DimGray;
            this.txtID.Location = new System.Drawing.Point(52, 65);
            this.txtID.Name = "txtID";
            this.txtID.ReadOnly = false;
            this.txtID.SelectionColor = System.Drawing.Color.DimGray;
            this.txtID.Size = new System.Drawing.Size(318, 27);
            this.txtID.TabIndex = 61;
            this.txtID.WordWrap = false;
            // 
            // lblD
            // 
            this.lblD.AutoSize = true;
            this.lblD.BackColor = System.Drawing.Color.Transparent;
            this.lblD.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblD.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.lblD.Location = new System.Drawing.Point(12, 74);
            this.lblD.Name = "lblD";
            this.lblD.Size = new System.Drawing.Size(18, 13);
            this.lblD.TabIndex = 59;
            this.lblD.Text = "ID";
            // 
            // txtJSON
            // 
            this.txtJSON.AutoWordSelection = false;
            this.txtJSON.BackColor = System.Drawing.Color.Transparent;
            this.txtJSON.Font = new System.Drawing.Font("Tahoma", 10F);
            this.txtJSON.ForeColor = System.Drawing.Color.DimGray;
            this.txtJSON.Location = new System.Drawing.Point(52, 32);
            this.txtJSON.Name = "txtJSON";
            this.txtJSON.ReadOnly = true;
            this.txtJSON.SelectionColor = System.Drawing.Color.DimGray;
            this.txtJSON.Size = new System.Drawing.Size(318, 27);
            this.txtJSON.TabIndex = 58;
            this.txtJSON.WordWrap = false;
            // 
            // lblJSON
            // 
            this.lblJSON.AutoSize = true;
            this.lblJSON.BackColor = System.Drawing.Color.Transparent;
            this.lblJSON.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblJSON.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.lblJSON.Location = new System.Drawing.Point(12, 41);
            this.lblJSON.Name = "lblJSON";
            this.lblJSON.Size = new System.Drawing.Size(34, 13);
            this.lblJSON.TabIndex = 50;
            this.lblJSON.Text = "JSON";
            // 
            // btnJSON
            // 
            this.btnJSON.BackColor = System.Drawing.Color.Transparent;
            this.btnJSON.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnJSON.Image = global::RecipeCard.Properties.Resources.folder;
            this.btnJSON.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnJSON.Location = new System.Drawing.Point(376, 32);
            this.btnJSON.Name = "btnJSON";
            this.btnJSON.Size = new System.Drawing.Size(38, 27);
            this.btnJSON.TabIndex = 51;
            this.btnJSON.TextAlignment = System.Drawing.StringAlignment.Center;
            this.btnJSON.Click += new System.EventHandler(this.btnJSON_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.AutoWordSelection = false;
            this.txtOutput.BackColor = System.Drawing.Color.Transparent;
            this.txtOutput.Font = new System.Drawing.Font("Tahoma", 10F);
            this.txtOutput.ForeColor = System.Drawing.Color.DimGray;
            this.txtOutput.Location = new System.Drawing.Point(6, 182);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.SelectionColor = System.Drawing.Color.DimGray;
            this.txtOutput.Size = new System.Drawing.Size(538, 287);
            this.txtOutput.TabIndex = 14;
            this.txtOutput.WordWrap = true;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.BackColor = System.Drawing.Color.Transparent;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStart.Image = null;
            this.btnStart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStart.Location = new System.Drawing.Point(420, 32);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 27);
            this.btnStart.TabIndex = 13;
            this.btnStart.Text = "START";
            this.btnStart.TextAlignment = System.Drawing.StringAlignment.Center;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // iTalk_ControlBox1
            // 
            this.iTalk_ControlBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.iTalk_ControlBox1.BackColor = System.Drawing.Color.Transparent;
            this.iTalk_ControlBox1.Location = new System.Drawing.Point(469, -1);
            this.iTalk_ControlBox1.Name = "iTalk_ControlBox1";
            this.iTalk_ControlBox1.Size = new System.Drawing.Size(77, 19);
            this.iTalk_ControlBox1.TabIndex = 0;
            this.iTalk_ControlBox1.Text = "iTalk_ControlBox1";
            // 
            // txtCfg
            // 
            this.txtCfg.AutoWordSelection = false;
            this.txtCfg.BackColor = System.Drawing.Color.Transparent;
            this.txtCfg.Font = new System.Drawing.Font("Tahoma", 10F);
            this.txtCfg.ForeColor = System.Drawing.Color.DimGray;
            this.txtCfg.Location = new System.Drawing.Point(52, 98);
            this.txtCfg.Name = "txtCfg";
            this.txtCfg.ReadOnly = true;
            this.txtCfg.SelectionColor = System.Drawing.Color.DimGray;
            this.txtCfg.Size = new System.Drawing.Size(318, 27);
            this.txtCfg.TabIndex = 92;
            this.txtCfg.WordWrap = false;
            // 
            // lblCfg
            // 
            this.lblCfg.AutoSize = true;
            this.lblCfg.BackColor = System.Drawing.Color.Transparent;
            this.lblCfg.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCfg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.lblCfg.Location = new System.Drawing.Point(12, 107);
            this.lblCfg.Name = "lblCfg";
            this.lblCfg.Size = new System.Drawing.Size(28, 13);
            this.lblCfg.TabIndex = 90;
            this.lblCfg.Text = "CFG";
            // 
            // btnCfg
            // 
            this.btnCfg.BackColor = System.Drawing.Color.Transparent;
            this.btnCfg.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnCfg.Image = global::RecipeCard.Properties.Resources.folder;
            this.btnCfg.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCfg.Location = new System.Drawing.Point(376, 98);
            this.btnCfg.Name = "btnCfg";
            this.btnCfg.Size = new System.Drawing.Size(38, 27);
            this.btnCfg.TabIndex = 91;
            this.btnCfg.TextAlignment = System.Drawing.StringAlignment.Center;
            this.btnCfg.Click += new System.EventHandler(this.btnCfg_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 500);
            this.Controls.Add(this.iTalk_ThemeContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(126, 39);
            this.Icon = new System.Drawing.Icon(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("RecipeCard.Resources.Icon.ico"));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Recipe Card";
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.iTalk_ThemeContainer1.ResumeLayout(false);
            this.iTalk_ThemeContainer1.PerformLayout();
            this.groupBoxPDF.ResumeLayout(false);
            this.groupBoxPDF.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private iTalk.iTalk_ControlBox iTalk_ControlBox1;
        private iTalk.iTalk_RichTextBox txtOutput;
        private iTalk.iTalk_ThemeContainer iTalk_ThemeContainer1;
        private iTalk.iTalk_RichTextBox txtJSON;
        private iTalk.iTalk_Label lblJSON;
        private iTalk.iTalk_Button_1 btnJSON;
        private iTalk.iTalk_Button_1 btnStart;
        private iTalk.iTalk_RichTextBox txtID;
        private iTalk.iTalk_Label lblD;
        private iTalk.iTalk_Button_1 btnId;
        private System.Windows.Forms.GroupBox groupBoxPDF;
        private iTalk.iTalk_Button_1 btnStartPDF;
        private iTalk.iTalk_RichTextBox txtJSONPDF;
        private iTalk.iTalk_Label lblJSONPDF;
        private iTalk.iTalk_Button_1 btnJSONPDF;
        private iTalk.iTalk_RichTextBox txtCfg;
        private iTalk.iTalk_Label lblCfg;
        private iTalk.iTalk_Button_1 btnCfg;
    }
}