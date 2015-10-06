namespace pico.Controls
{
	partial class TouchPad
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
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonFrameSelection = new System.Windows.Forms.Button();
			this.labelHint = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonFrameSelection
			// 
			this.buttonFrameSelection.Location = new System.Drawing.Point(0, 0);
			this.buttonFrameSelection.Name = "buttonFrameSelection";
			this.buttonFrameSelection.Size = new System.Drawing.Size(100, 23);
			this.buttonFrameSelection.TabIndex = 0;
			this.buttonFrameSelection.Text = "Frame Selection";
			this.buttonFrameSelection.UseVisualStyleBackColor = true;
			// 
			// labelHint
			// 
			this.labelHint.AutoSize = true;
			this.labelHint.Enabled = false;
			this.labelHint.Location = new System.Drawing.Point(0, 25);
			this.labelHint.Name = "labelHint";
			this.labelHint.Size = new System.Drawing.Size(100, 23);
			this.labelHint.TabIndex = 0;
			this.labelHint.Text = "label1";
			// 
			// TouchPad
			// 
			this.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonFrameSelection;
		private System.Windows.Forms.Label labelHint;

	}
}
