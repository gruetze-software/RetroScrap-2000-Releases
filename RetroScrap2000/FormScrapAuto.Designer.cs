﻿namespace RetroScrap2000
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
			checkBoxOnlyLocal = new CheckBox();
			statusStrip1 = new StatusStrip();
			toolStripStatusLabelProgress = new ToolStripStatusLabel();
			groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxAniWait).BeginInit();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// groupBox1
			// 
			resources.ApplyResources(groupBox1, "groupBox1");
			groupBox1.Controls.Add(listViewMonitor);
			groupBox1.Name = "groupBox1";
			groupBox1.TabStop = false;
			// 
			// listViewMonitor
			// 
			listViewMonitor.Columns.AddRange(new ColumnHeader[] { colTime, colTyp, colNr, colRom, colMsg });
			resources.ApplyResources(listViewMonitor, "listViewMonitor");
			listViewMonitor.FullRowSelect = true;
			listViewMonitor.GridLines = true;
			listViewMonitor.Name = "listViewMonitor";
			listViewMonitor.ShowGroups = false;
			listViewMonitor.UseCompatibleStateImageBehavior = false;
			listViewMonitor.View = View.Details;
			// 
			// colTime
			// 
			resources.ApplyResources(colTime, "colTime");
			// 
			// colTyp
			// 
			resources.ApplyResources(colTyp, "colTyp");
			// 
			// colNr
			// 
			resources.ApplyResources(colNr, "colNr");
			// 
			// colRom
			// 
			resources.ApplyResources(colRom, "colRom");
			// 
			// colMsg
			// 
			resources.ApplyResources(colMsg, "colMsg");
			// 
			// buttonStart
			// 
			resources.ApplyResources(buttonStart, "buttonStart");
			buttonStart.Name = "buttonStart";
			buttonStart.UseVisualStyleBackColor = true;
			buttonStart.Click += buttonStart_Click;
			// 
			// progressBarScrap
			// 
			resources.ApplyResources(progressBarScrap, "progressBarScrap");
			progressBarScrap.Name = "progressBarScrap";
			// 
			// pictureBoxAniWait
			// 
			resources.ApplyResources(pictureBoxAniWait, "pictureBoxAniWait");
			pictureBoxAniWait.Name = "pictureBoxAniWait";
			pictureBoxAniWait.TabStop = false;
			// 
			// checkBoxOnlyLocal
			// 
			resources.ApplyResources(checkBoxOnlyLocal, "checkBoxOnlyLocal");
			checkBoxOnlyLocal.Name = "checkBoxOnlyLocal";
			checkBoxOnlyLocal.UseVisualStyleBackColor = true;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelProgress });
			resources.ApplyResources(statusStrip1, "statusStrip1");
			statusStrip1.Name = "statusStrip1";
			// 
			// toolStripStatusLabelProgress
			// 
			toolStripStatusLabelProgress.Name = "toolStripStatusLabelProgress";
			resources.ApplyResources(toolStripStatusLabelProgress, "toolStripStatusLabelProgress");
			// 
			// FormScrapAuto
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(statusStrip1);
			Controls.Add(checkBoxOnlyLocal);
			Controls.Add(pictureBoxAniWait);
			Controls.Add(progressBarScrap);
			Controls.Add(buttonStart);
			Controls.Add(groupBox1);
			Name = "FormScrapAuto";
			FormClosing += FormScrapAuto_FormClosing;
			Load += FormScrapAuto_Load;
			groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxAniWait).EndInit();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
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
		private CheckBox checkBoxOnlyLocal;
		private StatusStrip statusStrip1;
		private ToolStripStatusLabel toolStripStatusLabelProgress;
	}
}