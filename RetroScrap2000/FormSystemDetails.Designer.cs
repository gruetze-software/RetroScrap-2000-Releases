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
			buttonOK.Location = new Point(395, 509);
			buttonOK.Name = "buttonOK";
			buttonOK.Size = new Size(94, 29);
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
			listViewDetails.Location = new Point(12, 111);
			listViewDetails.Name = "listViewDetails";
			listViewDetails.Size = new Size(477, 160);
			listViewDetails.TabIndex = 0;
			listViewDetails.UseCompatibleStateImageBehavior = false;
			listViewDetails.View = View.Details;
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBox1.Controls.Add(textBoxHistory);
			groupBox1.Location = new Point(12, 277);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(477, 226);
			groupBox1.TabIndex = 1;
			groupBox1.TabStop = false;
			groupBox1.Text = "History";
			// 
			// textBoxHistory
			// 
			textBoxHistory.AcceptsReturn = true;
			textBoxHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			textBoxHistory.Location = new Point(6, 26);
			textBoxHistory.Multiline = true;
			textBoxHistory.Name = "textBoxHistory";
			textBoxHistory.ScrollBars = ScrollBars.Vertical;
			textBoxHistory.Size = new Size(465, 194);
			textBoxHistory.TabIndex = 0;
			textBoxHistory.TextChanged += textBoxHistory_TextChanged;
			// 
			// pictureBoxSystem
			// 
			pictureBoxSystem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			pictureBoxSystem.Location = new Point(12, 12);
			pictureBoxSystem.Name = "pictureBoxSystem";
			pictureBoxSystem.Size = new Size(477, 83);
			pictureBoxSystem.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBoxSystem.TabIndex = 3;
			pictureBoxSystem.TabStop = false;
			// 
			// FormSystemDetails
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(501, 550);
			Controls.Add(pictureBoxSystem);
			Controls.Add(groupBox1);
			Controls.Add(listViewDetails);
			Controls.Add(buttonOK);
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