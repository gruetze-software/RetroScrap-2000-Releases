namespace RetroScrap2000
{
	partial class MediaPreviewControl
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
			tableLayoutPanelMain = new TableLayoutPanel();
			labelTitle = new Label();
			pictureBoxMedium = new PictureBox();
			checkBoxTakeOver = new CheckBox();
			tableLayoutPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxMedium).BeginInit();
			SuspendLayout();
			// 
			// tableLayoutPanelMain
			// 
			tableLayoutPanelMain.ColumnCount = 1;
			tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanelMain.Controls.Add(labelTitle, 0, 0);
			tableLayoutPanelMain.Controls.Add(pictureBoxMedium, 0, 1);
			tableLayoutPanelMain.Controls.Add(checkBoxTakeOver, 0, 2);
			tableLayoutPanelMain.Dock = DockStyle.Fill;
			tableLayoutPanelMain.Location = new Point(5, 5);
			tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			tableLayoutPanelMain.RowCount = 3;
			tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
			tableLayoutPanelMain.Size = new Size(150, 210);
			tableLayoutPanelMain.TabIndex = 0;
			// 
			// labelTitle
			// 
			labelTitle.AutoSize = true;
			labelTitle.Dock = DockStyle.Fill;
			labelTitle.Location = new Point(3, 0);
			labelTitle.Name = "labelTitle";
			labelTitle.Size = new Size(144, 20);
			labelTitle.TabIndex = 1;
			labelTitle.Text = "Media";
			labelTitle.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// pictureBoxMedium
			// 
			pictureBoxMedium.BackColor = Color.DimGray;
			pictureBoxMedium.Dock = DockStyle.Fill;
			pictureBoxMedium.Location = new Point(3, 23);
			pictureBoxMedium.Name = "pictureBoxMedium";
			pictureBoxMedium.Size = new Size(144, 159);
			pictureBoxMedium.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBoxMedium.TabIndex = 1;
			pictureBoxMedium.TabStop = false;
			// 
			// checkBoxTakeOver
			// 
			checkBoxTakeOver.Anchor = AnchorStyles.None;
			checkBoxTakeOver.AutoSize = true;
			checkBoxTakeOver.Location = new Point(38, 188);
			checkBoxTakeOver.Name = "checkBoxTakeOver";
			checkBoxTakeOver.Size = new Size(74, 19);
			checkBoxTakeOver.TabIndex = 0;
			checkBoxTakeOver.Text = "take over";
			checkBoxTakeOver.UseVisualStyleBackColor = true;
			// 
			// MediaPreviewControl
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(tableLayoutPanelMain);
			Name = "MediaPreviewControl";
			Padding = new Padding(5);
			Size = new Size(160, 220);
			tableLayoutPanelMain.ResumeLayout(false);
			tableLayoutPanelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxMedium).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel tableLayoutPanelMain;
		private Label labelTitle;
		private PictureBox pictureBoxMedium;
		private CheckBox checkBoxTakeOver;
	}
}
