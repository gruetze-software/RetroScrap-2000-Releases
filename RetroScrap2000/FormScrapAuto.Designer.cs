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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormScrapAuto));
			groupBox1 = new GroupBox();
			listViewMonitor = new ListView();
			columnHeader1 = new ColumnHeader();
			columnHeader4 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			columnHeader3 = new ColumnHeader();
			buttonStart = new Button();
			buttonStopp = new Button();
			progressBarScrap = new ProgressBar();
			labelScrap = new Label();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBox1.Controls.Add(listViewMonitor);
			groupBox1.Location = new Point(12, 57);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(776, 434);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox1.Text = "Monitor";
			// 
			// listViewMonitor
			// 
			listViewMonitor.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader4, columnHeader2, columnHeader3 });
			listViewMonitor.Dock = DockStyle.Fill;
			listViewMonitor.FullRowSelect = true;
			listViewMonitor.GridLines = true;
			listViewMonitor.Location = new Point(3, 23);
			listViewMonitor.Name = "listViewMonitor";
			listViewMonitor.ShowGroups = false;
			listViewMonitor.Size = new Size(770, 408);
			listViewMonitor.TabIndex = 0;
			listViewMonitor.UseCompatibleStateImageBehavior = false;
			listViewMonitor.View = View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Zeitpunkt";
			columnHeader1.Width = 100;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Nr.";
			columnHeader4.Width = 90;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Typ";
			columnHeader2.Width = 90;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Message";
			columnHeader3.Width = 400;
			// 
			// buttonStart
			// 
			buttonStart.Location = new Point(12, 12);
			buttonStart.Name = "buttonStart";
			buttonStart.Size = new Size(172, 39);
			buttonStart.TabIndex = 2;
			buttonStart.Text = "Starte Scrapping";
			buttonStart.UseVisualStyleBackColor = true;
			buttonStart.Click += buttonStart_Click;
			// 
			// buttonStopp
			// 
			buttonStopp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			buttonStopp.Location = new Point(613, 12);
			buttonStopp.Name = "buttonStopp";
			buttonStopp.Size = new Size(172, 39);
			buttonStopp.TabIndex = 3;
			buttonStopp.Text = "Stopp";
			buttonStopp.UseVisualStyleBackColor = true;
			buttonStopp.Click += buttonStopp_Click;
			// 
			// progressBarScrap
			// 
			progressBarScrap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			progressBarScrap.Location = new Point(203, 12);
			progressBarScrap.Name = "progressBarScrap";
			progressBarScrap.Size = new Size(313, 39);
			progressBarScrap.TabIndex = 4;
			// 
			// labelScrap
			// 
			labelScrap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelScrap.AutoSize = true;
			labelScrap.Location = new Point(552, 21);
			labelScrap.Name = "labelScrap";
			labelScrap.Size = new Size(31, 20);
			labelScrap.TabIndex = 5;
			labelScrap.Text = "0/0";
			// 
			// FormScrapAuto
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 503);
			Controls.Add(labelScrap);
			Controls.Add(progressBarScrap);
			Controls.Add(buttonStopp);
			Controls.Add(buttonStart);
			Controls.Add(groupBox1);
			Name = "FormScrapAuto";
			StartPosition = FormStartPosition.CenterParent;
			Text = "FormScrapAuto";
			Load += FormScrapAuto_Load;
			groupBox1.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private GroupBox groupBox1;
		private Button buttonStart;
		private Button buttonStopp;
		private ListView listViewMonitor;
		private ProgressBar progressBarScrap;
		private Label labelScrap;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
	}
}