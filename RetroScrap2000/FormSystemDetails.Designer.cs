namespace RetroScrap2000
{
	partial class FormSystemDetails
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
			buttonOK = new Button();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			listViewDetails = new ListView();
			groupBox1 = new GroupBox();
			textBoxHistory = new TextBox();
			pictureBoxSystem = new PictureBox();
			groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxSystem).BeginInit();
			SuspendLayout();
			// 
			// buttonOK
			// 
			buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonOK.DialogResult = DialogResult.OK;
			buttonOK.Location = new Point(346, 438);
			buttonOK.Margin = new Padding(3, 2, 3, 2);
			buttonOK.Name = "buttonOK";
			buttonOK.Size = new Size(82, 22);
			buttonOK.TabIndex = 2;
			buttonOK.Text = "OK";
			buttonOK.UseVisualStyleBackColor = true;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Key";
			columnHeader1.Width = 130;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Value";
			columnHeader2.Width = 250;
			// 
			// listViewDetails
			// 
			listViewDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			listViewDetails.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
			listViewDetails.FullRowSelect = true;
			listViewDetails.GridLines = true;
			listViewDetails.Location = new Point(10, 83);
			listViewDetails.Margin = new Padding(3, 2, 3, 2);
			listViewDetails.Name = "listViewDetails";
			listViewDetails.Size = new Size(418, 164);
			listViewDetails.TabIndex = 0;
			listViewDetails.UseCompatibleStateImageBehavior = false;
			listViewDetails.View = View.Details;
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBox1.Controls.Add(textBoxHistory);
			groupBox1.Location = new Point(10, 251);
			groupBox1.Margin = new Padding(3, 2, 3, 2);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new Padding(3, 2, 3, 2);
			groupBox1.Size = new Size(417, 183);
			groupBox1.TabIndex = 1;
			groupBox1.TabStop = false;
			groupBox1.Text = "History";
			// 
			// textBoxHistory
			// 
			textBoxHistory.AcceptsReturn = true;
			textBoxHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			textBoxHistory.Location = new Point(5, 20);
			textBoxHistory.Margin = new Padding(3, 2, 3, 2);
			textBoxHistory.Multiline = true;
			textBoxHistory.Name = "textBoxHistory";
			textBoxHistory.ScrollBars = ScrollBars.Vertical;
			textBoxHistory.Size = new Size(407, 159);
			textBoxHistory.TabIndex = 0;
			textBoxHistory.TextChanged += textBoxHistory_TextChanged;
			// 
			// pictureBoxSystem
			// 
			pictureBoxSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			pictureBoxSystem.Location = new Point(10, 9);
			pictureBoxSystem.Margin = new Padding(3, 2, 3, 2);
			pictureBoxSystem.Name = "pictureBoxSystem";
			pictureBoxSystem.Size = new Size(417, 62);
			pictureBoxSystem.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBoxSystem.TabIndex = 3;
			pictureBoxSystem.TabStop = false;
			// 
			// FormSystemDetails
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(438, 468);
			Controls.Add(pictureBoxSystem);
			Controls.Add(groupBox1);
			Controls.Add(listViewDetails);
			Controls.Add(buttonOK);
			Margin = new Padding(3, 2, 3, 2);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormSystemDetails";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Details";
			Load += FormSystemDetails_Load;
			Resize += FormSystemDetails_Resize;
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxSystem).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Button buttonOK;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ListView listViewDetails;
		private GroupBox groupBox1;
		private TextBox textBoxHistory;
		private PictureBox pictureBoxSystem;
	}
}