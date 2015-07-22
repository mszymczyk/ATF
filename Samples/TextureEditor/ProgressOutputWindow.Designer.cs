namespace TextureEditor
{
	partial class ProgressOutputWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.CloseButtonObj = new System.Windows.Forms.Button();
			this.CancelButtonObj = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.richTextBox1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1164, 412);
			this.panel1.TabIndex = 1;
			// 
			// richTextBox1
			// 
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Location = new System.Drawing.Point(0, 0);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(1164, 412);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// 
			// progressBar1
			// 
			this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.progressBar1.Location = new System.Drawing.Point(0, 0);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(995, 33);
			this.progressBar1.TabIndex = 2;
			// 
			// CloseButtonObj
			// 
			this.CloseButtonObj.Enabled = false;
			this.CloseButtonObj.Location = new System.Drawing.Point(3, 3);
			this.CloseButtonObj.Name = "CloseButtonObj";
			this.CloseButtonObj.Size = new System.Drawing.Size(75, 23);
			this.CloseButtonObj.TabIndex = 3;
			this.CloseButtonObj.Text = "Close";
			this.CloseButtonObj.UseVisualStyleBackColor = true;
			this.CloseButtonObj.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// CancelButtonObj
			// 
			this.CancelButtonObj.Location = new System.Drawing.Point(84, 3);
			this.CancelButtonObj.Name = "CancelButtonObj";
			this.CancelButtonObj.Size = new System.Drawing.Size(75, 23);
			this.CancelButtonObj.TabIndex = 4;
			this.CancelButtonObj.Text = "Cancel";
			this.CancelButtonObj.UseVisualStyleBackColor = true;
			this.CancelButtonObj.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.progressBar1);
			this.panel2.Controls.Add(this.flowLayoutPanel1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 379);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1164, 33);
			this.panel2.TabIndex = 5;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.flowLayoutPanel1.Controls.Add(this.CloseButtonObj);
			this.flowLayoutPanel1.Controls.Add(this.CancelButtonObj);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(995, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(169, 33);
			this.flowLayoutPanel1.TabIndex = 3;
			// 
			// ProgressOutputWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1164, 412);
			this.ControlBox = false;
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressOutputWindow";
			this.ShowIcon = false;
			this.Text = "ProgressOutputWindow";
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button CloseButtonObj;
		private System.Windows.Forms.Button CancelButtonObj;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

	}
}