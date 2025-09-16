namespace RetroScrap2000
{
	partial class FormSplashScreen
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
			pictureBoxAnimation = new PictureBox();
			labelStatus = new Label();
			progressBar = new ProgressBar();
			tableLayoutPanel1 = new TableLayoutPanel();
			tableLayoutPanel2 = new TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)pictureBoxAnimation).BeginInit();
			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			SuspendLayout();
			// 
			// pictureBoxAnimation
			// 
			pictureBoxAnimation.Dock = DockStyle.Fill;
			pictureBoxAnimation.Location = new Point(13, 13);
			pictureBoxAnimation.Margin = new Padding(10);
			pictureBoxAnimation.Name = "pictureBoxAnimation";
			pictureBoxAnimation.Size = new Size(133, 120);
			pictureBoxAnimation.TabIndex = 0;
			pictureBoxAnimation.TabStop = false;
			// 
			// labelStatus
			// 
			labelStatus.Dock = DockStyle.Fill;
			labelStatus.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
			labelStatus.Location = new Point(12, 8);
			labelStatus.Margin = new Padding(12, 8, 24, 8);
			labelStatus.Name = "labelStatus";
			labelStatus.Padding = new Padding(8);
			labelStatus.Size = new Size(329, 77);
			labelStatus.TabIndex = 1;
			labelStatus.Text = "Status";
			labelStatus.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// progressBar
			// 
			progressBar.Dock = DockStyle.Fill;
			progressBar.Location = new Point(3, 96);
			progressBar.Name = "progressBar";
			progressBar.Size = new Size(359, 35);
			progressBar.TabIndex = 2;
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.Anchor = AnchorStyles.None;
			tableLayoutPanel1.BackColor = Color.White;
			tableLayoutPanel1.ColumnCount = 2;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 0);
			tableLayoutPanel1.Controls.Add(pictureBoxAnimation, 0, 0);
			tableLayoutPanel1.Location = new Point(2, 2);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.Padding = new Padding(3);
			tableLayoutPanel1.RowCount = 1;
			tableLayoutPanel1.RowStyles.Add(new RowStyle());
			tableLayoutPanel1.Size = new Size(516, 144);
			tableLayoutPanel1.TabIndex = 3;
			// 
			// tableLayoutPanel2
			// 
			tableLayoutPanel2.ColumnCount = 1;
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
			tableLayoutPanel2.Controls.Add(labelStatus, 0, 0);
			tableLayoutPanel2.Controls.Add(progressBar, 0, 1);
			tableLayoutPanel2.Dock = DockStyle.Fill;
			tableLayoutPanel2.Location = new Point(159, 6);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 2;
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
			tableLayoutPanel2.Size = new Size(351, 134);
			tableLayoutPanel2.TabIndex = 1;
			// 
			// FormSplashScreen
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.Silver;
			ClientSize = new Size(520, 148);
			Controls.Add(tableLayoutPanel1);
			FormBorderStyle = FormBorderStyle.None;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormSplashScreen";
			StartPosition = FormStartPosition.CenterParent;
			Text = "SplashScreen";
			((System.ComponentModel.ISupportInitialize)pictureBoxAnimation).EndInit();
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel2.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private PictureBox pictureBoxAnimation;
		private Label labelStatus;
		private ProgressBar progressBar;
		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel2;
	}
}