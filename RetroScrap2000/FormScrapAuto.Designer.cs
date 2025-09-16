namespace RetroScrap2000
{
	partial class FormScrapAuto
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormScrapAuto));
			groupBox1 = new GroupBox();
			listViewMonitor = new ListView();
			colTime = new ColumnHeader();
			colTyp = new ColumnHeader();
			colNr = new ColumnHeader();
			colRom = new ColumnHeader();
			colMsg = new ColumnHeader();
			buttonStart = new Button();
			progressBarScrap = new ProgressBar();
			pictureBox1 = new PictureBox();
			groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBox1.Controls.Add(listViewMonitor);
			groupBox1.Location = new Point(12, 80);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(874, 411);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox1.Text = "Monitor";
			// 
			// listViewMonitor
			// 
			listViewMonitor.Columns.AddRange(new ColumnHeader[] { colTime, colTyp, colNr, colRom, colMsg });
			listViewMonitor.Dock = DockStyle.Fill;
			listViewMonitor.FullRowSelect = true;
			listViewMonitor.GridLines = true;
			listViewMonitor.Location = new Point(3, 23);
			listViewMonitor.Name = "listViewMonitor";
			listViewMonitor.ShowGroups = false;
			listViewMonitor.Size = new Size(868, 385);
			listViewMonitor.TabIndex = 0;
			listViewMonitor.UseCompatibleStateImageBehavior = false;
			listViewMonitor.View = View.Details;
			// 
			// colTime
			// 
			colTime.Text = "Zeitpunkt";
			colTime.Width = 100;
			// 
			// colTyp
			// 
			colTyp.Text = "Typ";
			colTyp.Width = 90;
			// 
			// colNr
			// 
			colNr.Text = "Nr.";
			colNr.Width = 90;
			// 
			// colRom
			// 
			colRom.Text = "Rom";
			colRom.Width = 150;
			// 
			// colMsg
			// 
			colMsg.Text = "Message";
			colMsg.Width = 400;
			// 
			// buttonStart
			// 
			buttonStart.Location = new Point(12, 12);
			buttonStart.Name = "buttonStart";
			buttonStart.Size = new Size(185, 62);
			buttonStart.TabIndex = 2;
			buttonStart.Text = "Start";
			buttonStart.UseVisualStyleBackColor = true;
			buttonStart.Click += buttonStart_Click;
			// 
			// progressBarScrap
			// 
			progressBarScrap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			progressBarScrap.Location = new Point(203, 12);
			progressBarScrap.Name = "progressBarScrap";
			progressBarScrap.Size = new Size(599, 62);
			progressBarScrap.TabIndex = 4;
			// 
			// pictureBox1
			// 
			pictureBox1.Location = new Point(808, 12);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(75, 62);
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.TabIndex = 6;
			pictureBox1.TabStop = false;
			// 
			// FormScrapAuto
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(898, 503);
			Controls.Add(pictureBox1);
			Controls.Add(progressBarScrap);
			Controls.Add(buttonStart);
			Controls.Add(groupBox1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Name = "FormScrapAuto";
			StartPosition = FormStartPosition.CenterParent;
			Text = "FormScrapAuto";
			Load += FormScrapAuto_Load;
			groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private GroupBox groupBox1;
		private Button buttonStart;
		private ListView listViewMonitor;
		private ProgressBar progressBarScrap;
		private ColumnHeader colTime;
		private ColumnHeader colTyp;
		private ColumnHeader colMsg;
		private ColumnHeader colNr;
		private ColumnHeader colRom;
		private PictureBox pictureBox1;
	}
}