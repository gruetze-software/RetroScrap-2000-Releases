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
			pictureBoxAniWait = new PictureBox();
			groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxAniWait).BeginInit();
			SuspendLayout();
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBox1.Controls.Add(listViewMonitor);
			groupBox1.Location = new Point(10, 60);
			groupBox1.Margin = new Padding(3, 2, 3, 2);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new Padding(3, 2, 3, 2);
			groupBox1.Size = new Size(856, 408);
			groupBox1.TabIndex = 2;
			groupBox1.TabStop = false;
			groupBox1.Text = "Monitor";
			// 
			// listViewMonitor
			// 
			listViewMonitor.Columns.AddRange(new ColumnHeader[] { colTime, colTyp, colNr, colRom, colMsg });
			listViewMonitor.Dock = DockStyle.Fill;
			listViewMonitor.FullRowSelect = true;
			listViewMonitor.GridLines = true;
			listViewMonitor.Location = new Point(3, 18);
			listViewMonitor.Margin = new Padding(3, 2, 3, 2);
			listViewMonitor.Name = "listViewMonitor";
			listViewMonitor.ShowGroups = false;
			listViewMonitor.Size = new Size(850, 388);
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
			buttonStart.Location = new Point(10, 9);
			buttonStart.Margin = new Padding(3, 2, 3, 2);
			buttonStart.Name = "buttonStart";
			buttonStart.Size = new Size(162, 46);
			buttonStart.TabIndex = 0;
			buttonStart.Text = "Start";
			buttonStart.UseVisualStyleBackColor = true;
			buttonStart.Click += buttonStart_Click;
			// 
			// progressBarScrap
			// 
			progressBarScrap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			progressBarScrap.Location = new Point(178, 9);
			progressBarScrap.Margin = new Padding(3, 2, 3, 2);
			progressBarScrap.Name = "progressBarScrap";
			progressBarScrap.Size = new Size(615, 46);
			progressBarScrap.TabIndex = 1;
			// 
			// pictureBoxAniWait
			// 
			pictureBoxAniWait.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			pictureBoxAniWait.Location = new Point(797, 9);
			pictureBoxAniWait.Margin = new Padding(3, 2, 3, 2);
			pictureBoxAniWait.Name = "pictureBoxAniWait";
			pictureBoxAniWait.Size = new Size(66, 46);
			pictureBoxAniWait.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBoxAniWait.TabIndex = 6;
			pictureBoxAniWait.TabStop = false;
			// 
			// FormScrapAuto
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(877, 477);
			Controls.Add(pictureBoxAniWait);
			Controls.Add(progressBarScrap);
			Controls.Add(buttonStart);
			Controls.Add(groupBox1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(3, 2, 3, 2);
			Name = "FormScrapAuto";
			StartPosition = FormStartPosition.CenterParent;
			Text = "FormScrapAuto";
			Load += FormScrapAuto_Load;
			groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxAniWait).EndInit();
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
		private PictureBox pictureBoxAniWait;
	}
}