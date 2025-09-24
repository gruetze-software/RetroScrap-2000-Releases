namespace RetroScrap2000
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			splitContainerMain = new SplitContainer();
			tableLayoutPanelMainLeft = new TableLayoutPanel();
			listViewSystems = new ListView();
			colSystemName = new ColumnHeader();
			colSystemHersteller = new ColumnHeader();
			colSystemVon = new ColumnHeader();
			colSystemBis = new ColumnHeader();
			colSystemAnzRoms = new ColumnHeader();
			contextMenuStripSystems = new ContextMenuStrip(components);
			alleRomsScrapenToolStripMenuItem = new ToolStripMenuItem();
			detailsToolStripMenuItem = new ToolStripMenuItem();
			tableLayoutPanelRomsControls = new TableLayoutPanel();
			buttonRomPath = new Button();
			buttonRomsRead = new Button();
			buttonOptions = new Button();
			listViewRoms = new ListView();
			colRomsName = new ColumnHeader();
			colRomsRelease = new ColumnHeader();
			colRomsGenre = new ColumnHeader();
			colRomsAnzPlayer = new ColumnHeader();
			colRomsRating = new ColumnHeader();
			colRomsFile = new ColumnHeader();
			contextMenuStripRoms = new ContextMenuStrip(components);
			scrapToolStripMenuItem = new ToolStripMenuItem();
			detailsToolStripMenuItem1 = new ToolStripMenuItem();
			löschenToolStripMenuItem = new ToolStripMenuItem();
			splitContainerRight = new SplitContainer();
			splitContainerRightRom = new SplitContainer();
			tableLayoutPanelRightInnen = new TableLayoutPanel();
			textBoxRomName = new TextBox();
			tableLayoutPanelSystem = new TableLayoutPanel();
			pictureBoxRomSystem = new PictureBox();
			listBoxSystem = new ListBox();
			textBoxRomDesc = new TextBox();
			tableLayoutPanelRomMedia = new TableLayoutPanel();
			pictureBoxImgBox = new PictureBox();
			contextMenuStripMedia = new ContextMenuStrip(components);
			showToolStripMenuItem = new ToolStripMenuItem();
			addToolStripMenuItem = new ToolStripMenuItem();
			deleteToolStripMenuItem = new ToolStripMenuItem();
			label6 = new Label();
			pictureBoxImgScreenshot = new PictureBox();
			label7 = new Label();
			pictureBoxImgVideo = new PictureBox();
			label8 = new Label();
			tableLayoutPanelRomDetails = new TableLayoutPanel();
			textBoxRomDetailsAnzPlayer = new TextBox();
			label10 = new Label();
			textBoxRomDetailsGenre = new TextBox();
			label9 = new Label();
			textBoxRomDetailsPublisher = new TextBox();
			label5 = new Label();
			textBoxRomDetailsDeveloper = new TextBox();
			label4 = new Label();
			label3 = new Label();
			label2 = new Label();
			textBoxRomDetailsReleaseDate = new TextBox();
			buttonRomSave = new Button();
			buttonRomScrap = new Button();
			starRatingControlRom = new StarRatingControl();
			statusStripMain = new StatusStrip();
			toolStripStatusLabelMain = new ToolStripStatusLabel();
			((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
			splitContainerMain.Panel1.SuspendLayout();
			splitContainerMain.Panel2.SuspendLayout();
			splitContainerMain.SuspendLayout();
			tableLayoutPanelMainLeft.SuspendLayout();
			contextMenuStripSystems.SuspendLayout();
			tableLayoutPanelRomsControls.SuspendLayout();
			contextMenuStripRoms.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainerRight).BeginInit();
			splitContainerRight.Panel1.SuspendLayout();
			splitContainerRight.Panel2.SuspendLayout();
			splitContainerRight.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainerRightRom).BeginInit();
			splitContainerRightRom.Panel1.SuspendLayout();
			splitContainerRightRom.Panel2.SuspendLayout();
			splitContainerRightRom.SuspendLayout();
			tableLayoutPanelRightInnen.SuspendLayout();
			tableLayoutPanelSystem.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxRomSystem).BeginInit();
			tableLayoutPanelRomMedia.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxImgBox).BeginInit();
			contextMenuStripMedia.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxImgScreenshot).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureBoxImgVideo).BeginInit();
			tableLayoutPanelRomDetails.SuspendLayout();
			statusStripMain.SuspendLayout();
			SuspendLayout();
			// 
			// splitContainerMain
			// 
			splitContainerMain.BorderStyle = BorderStyle.Fixed3D;
			resources.ApplyResources(splitContainerMain, "splitContainerMain");
			splitContainerMain.Name = "splitContainerMain";
			// 
			// splitContainerMain.Panel1
			// 
			splitContainerMain.Panel1.Controls.Add(tableLayoutPanelMainLeft);
			// 
			// splitContainerMain.Panel2
			// 
			splitContainerMain.Panel2.Controls.Add(splitContainerRight);
			// 
			// tableLayoutPanelMainLeft
			// 
			resources.ApplyResources(tableLayoutPanelMainLeft, "tableLayoutPanelMainLeft");
			tableLayoutPanelMainLeft.Controls.Add(listViewSystems, 0, 1);
			tableLayoutPanelMainLeft.Controls.Add(tableLayoutPanelRomsControls, 0, 0);
			tableLayoutPanelMainLeft.Controls.Add(listViewRoms, 0, 2);
			tableLayoutPanelMainLeft.Name = "tableLayoutPanelMainLeft";
			// 
			// listViewSystems
			// 
			listViewSystems.Activation = ItemActivation.OneClick;
			listViewSystems.Columns.AddRange(new ColumnHeader[] { colSystemName, colSystemHersteller, colSystemVon, colSystemBis, colSystemAnzRoms });
			listViewSystems.ContextMenuStrip = contextMenuStripSystems;
			resources.ApplyResources(listViewSystems, "listViewSystems");
			listViewSystems.FullRowSelect = true;
			listViewSystems.GridLines = true;
			listViewSystems.MultiSelect = false;
			listViewSystems.Name = "listViewSystems";
			listViewSystems.UseCompatibleStateImageBehavior = false;
			listViewSystems.View = View.Details;
			listViewSystems.ColumnClick += listViewSystems_ColumnClick;
			listViewSystems.SelectedIndexChanged += listViewSystems_SelectedIndexChanged;
			// 
			// colSystemName
			// 
			resources.ApplyResources(colSystemName, "colSystemName");
			// 
			// colSystemHersteller
			// 
			resources.ApplyResources(colSystemHersteller, "colSystemHersteller");
			// 
			// colSystemVon
			// 
			resources.ApplyResources(colSystemVon, "colSystemVon");
			// 
			// colSystemBis
			// 
			resources.ApplyResources(colSystemBis, "colSystemBis");
			// 
			// colSystemAnzRoms
			// 
			resources.ApplyResources(colSystemAnzRoms, "colSystemAnzRoms");
			// 
			// contextMenuStripSystems
			// 
			contextMenuStripSystems.ImageScalingSize = new Size(20, 20);
			contextMenuStripSystems.Items.AddRange(new ToolStripItem[] { alleRomsScrapenToolStripMenuItem, detailsToolStripMenuItem });
			contextMenuStripSystems.Name = "contextMenuStripSystems";
			resources.ApplyResources(contextMenuStripSystems, "contextMenuStripSystems");
			// 
			// alleRomsScrapenToolStripMenuItem
			// 
			alleRomsScrapenToolStripMenuItem.Name = "alleRomsScrapenToolStripMenuItem";
			resources.ApplyResources(alleRomsScrapenToolStripMenuItem, "alleRomsScrapenToolStripMenuItem");
			alleRomsScrapenToolStripMenuItem.Click += SystemAllRomsScrapToolStripMenuItem_Click;
			// 
			// detailsToolStripMenuItem
			// 
			resources.ApplyResources(detailsToolStripMenuItem, "detailsToolStripMenuItem");
			detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
			// 
			// tableLayoutPanelRomsControls
			// 
			resources.ApplyResources(tableLayoutPanelRomsControls, "tableLayoutPanelRomsControls");
			tableLayoutPanelRomsControls.Controls.Add(buttonRomPath, 0, 0);
			tableLayoutPanelRomsControls.Controls.Add(buttonRomsRead, 1, 0);
			tableLayoutPanelRomsControls.Controls.Add(buttonOptions, 2, 0);
			tableLayoutPanelRomsControls.Name = "tableLayoutPanelRomsControls";
			// 
			// buttonRomPath
			// 
			resources.ApplyResources(buttonRomPath, "buttonRomPath");
			buttonRomPath.Image = Properties.Resources.folderopen32;
			buttonRomPath.Name = "buttonRomPath";
			buttonRomPath.UseVisualStyleBackColor = true;
			buttonRomPath.Click += buttonRomPath_Click;
			// 
			// buttonRomsRead
			// 
			resources.ApplyResources(buttonRomsRead, "buttonRomsRead");
			buttonRomsRead.Image = Properties.Resources.scanrom_32;
			buttonRomsRead.Name = "buttonRomsRead";
			buttonRomsRead.UseVisualStyleBackColor = true;
			buttonRomsRead.Click += buttonRomsRead_Click;
			// 
			// buttonOptions
			// 
			resources.ApplyResources(buttonOptions, "buttonOptions");
			buttonOptions.Image = Properties.Resources.options_32;
			buttonOptions.Name = "buttonOptions";
			buttonOptions.UseVisualStyleBackColor = true;
			buttonOptions.Click += buttonOptions_Click;
			// 
			// listViewRoms
			// 
			listViewRoms.Activation = ItemActivation.OneClick;
			listViewRoms.Columns.AddRange(new ColumnHeader[] { colRomsName, colRomsRelease, colRomsGenre, colRomsAnzPlayer, colRomsRating, colRomsFile });
			listViewRoms.ContextMenuStrip = contextMenuStripRoms;
			resources.ApplyResources(listViewRoms, "listViewRoms");
			listViewRoms.FullRowSelect = true;
			listViewRoms.GridLines = true;
			listViewRoms.MultiSelect = false;
			listViewRoms.Name = "listViewRoms";
			listViewRoms.UseCompatibleStateImageBehavior = false;
			listViewRoms.View = View.Details;
			listViewRoms.ColumnClick += listViewRoms_ColumnClick;
			listViewRoms.SelectedIndexChanged += listViewRoms_SelectedIndexChanged;
			// 
			// colRomsName
			// 
			resources.ApplyResources(colRomsName, "colRomsName");
			// 
			// colRomsRelease
			// 
			resources.ApplyResources(colRomsRelease, "colRomsRelease");
			// 
			// colRomsGenre
			// 
			resources.ApplyResources(colRomsGenre, "colRomsGenre");
			// 
			// colRomsAnzPlayer
			// 
			resources.ApplyResources(colRomsAnzPlayer, "colRomsAnzPlayer");
			// 
			// colRomsRating
			// 
			resources.ApplyResources(colRomsRating, "colRomsRating");
			// 
			// colRomsFile
			// 
			resources.ApplyResources(colRomsFile, "colRomsFile");
			// 
			// contextMenuStripRoms
			// 
			contextMenuStripRoms.ImageScalingSize = new Size(20, 20);
			contextMenuStripRoms.Items.AddRange(new ToolStripItem[] { scrapToolStripMenuItem, detailsToolStripMenuItem1, löschenToolStripMenuItem });
			contextMenuStripRoms.Name = "contextMenuStripRoms";
			resources.ApplyResources(contextMenuStripRoms, "contextMenuStripRoms");
			// 
			// scrapToolStripMenuItem
			// 
			scrapToolStripMenuItem.Name = "scrapToolStripMenuItem";
			resources.ApplyResources(scrapToolStripMenuItem, "scrapToolStripMenuItem");
			scrapToolStripMenuItem.Click += RomScrapToolStripMenuItem_Click;
			// 
			// detailsToolStripMenuItem1
			// 
			detailsToolStripMenuItem1.Name = "detailsToolStripMenuItem1";
			resources.ApplyResources(detailsToolStripMenuItem1, "detailsToolStripMenuItem1");
			detailsToolStripMenuItem1.Click += RomDetailsToolStripMenuItem_Click;
			// 
			// löschenToolStripMenuItem
			// 
			löschenToolStripMenuItem.Name = "löschenToolStripMenuItem";
			resources.ApplyResources(löschenToolStripMenuItem, "löschenToolStripMenuItem");
			löschenToolStripMenuItem.Click += RomLöschenToolStripMenuItem_Click;
			// 
			// splitContainerRight
			// 
			resources.ApplyResources(splitContainerRight, "splitContainerRight");
			splitContainerRight.FixedPanel = FixedPanel.Panel2;
			splitContainerRight.Name = "splitContainerRight";
			// 
			// splitContainerRight.Panel1
			// 
			splitContainerRight.Panel1.Controls.Add(splitContainerRightRom);
			// 
			// splitContainerRight.Panel2
			// 
			splitContainerRight.Panel2.Controls.Add(tableLayoutPanelRomDetails);
			// 
			// splitContainerRightRom
			// 
			splitContainerRightRom.BorderStyle = BorderStyle.Fixed3D;
			resources.ApplyResources(splitContainerRightRom, "splitContainerRightRom");
			splitContainerRightRom.Name = "splitContainerRightRom";
			// 
			// splitContainerRightRom.Panel1
			// 
			splitContainerRightRom.Panel1.Controls.Add(tableLayoutPanelRightInnen);
			// 
			// splitContainerRightRom.Panel2
			// 
			splitContainerRightRom.Panel2.Controls.Add(tableLayoutPanelRomMedia);
			// 
			// tableLayoutPanelRightInnen
			// 
			resources.ApplyResources(tableLayoutPanelRightInnen, "tableLayoutPanelRightInnen");
			tableLayoutPanelRightInnen.Controls.Add(textBoxRomName, 0, 1);
			tableLayoutPanelRightInnen.Controls.Add(tableLayoutPanelSystem, 0, 0);
			tableLayoutPanelRightInnen.Controls.Add(textBoxRomDesc, 0, 2);
			tableLayoutPanelRightInnen.Name = "tableLayoutPanelRightInnen";
			// 
			// textBoxRomName
			// 
			resources.ApplyResources(textBoxRomName, "textBoxRomName");
			textBoxRomName.Name = "textBoxRomName";
			// 
			// tableLayoutPanelSystem
			// 
			resources.ApplyResources(tableLayoutPanelSystem, "tableLayoutPanelSystem");
			tableLayoutPanelSystem.Controls.Add(pictureBoxRomSystem, 1, 0);
			tableLayoutPanelSystem.Controls.Add(listBoxSystem, 0, 0);
			tableLayoutPanelSystem.Name = "tableLayoutPanelSystem";
			// 
			// pictureBoxRomSystem
			// 
			resources.ApplyResources(pictureBoxRomSystem, "pictureBoxRomSystem");
			pictureBoxRomSystem.Name = "pictureBoxRomSystem";
			pictureBoxRomSystem.TabStop = false;
			// 
			// listBoxSystem
			// 
			listBoxSystem.BackColor = SystemColors.Control;
			listBoxSystem.BorderStyle = BorderStyle.None;
			resources.ApplyResources(listBoxSystem, "listBoxSystem");
			listBoxSystem.FormattingEnabled = true;
			listBoxSystem.Name = "listBoxSystem";
			// 
			// textBoxRomDesc
			// 
			resources.ApplyResources(textBoxRomDesc, "textBoxRomDesc");
			textBoxRomDesc.Name = "textBoxRomDesc";
			// 
			// tableLayoutPanelRomMedia
			// 
			resources.ApplyResources(tableLayoutPanelRomMedia, "tableLayoutPanelRomMedia");
			tableLayoutPanelRomMedia.Controls.Add(pictureBoxImgBox, 0, 1);
			tableLayoutPanelRomMedia.Controls.Add(label6, 0, 0);
			tableLayoutPanelRomMedia.Controls.Add(pictureBoxImgScreenshot, 1, 1);
			tableLayoutPanelRomMedia.Controls.Add(label7, 1, 0);
			tableLayoutPanelRomMedia.Controls.Add(pictureBoxImgVideo, 2, 1);
			tableLayoutPanelRomMedia.Controls.Add(label8, 2, 0);
			tableLayoutPanelRomMedia.Name = "tableLayoutPanelRomMedia";
			// 
			// pictureBoxImgBox
			// 
			pictureBoxImgBox.BackColor = SystemColors.ControlLight;
			pictureBoxImgBox.ContextMenuStrip = contextMenuStripMedia;
			resources.ApplyResources(pictureBoxImgBox, "pictureBoxImgBox");
			pictureBoxImgBox.Name = "pictureBoxImgBox";
			pictureBoxImgBox.TabStop = false;
			pictureBoxImgBox.Click += pictureBoxImgVideo_Click;
			// 
			// contextMenuStripMedia
			// 
			contextMenuStripMedia.ImageScalingSize = new Size(20, 20);
			contextMenuStripMedia.Items.AddRange(new ToolStripItem[] { showToolStripMenuItem, addToolStripMenuItem, deleteToolStripMenuItem });
			contextMenuStripMedia.Name = "contextMenuStripMedia";
			resources.ApplyResources(contextMenuStripMedia, "contextMenuStripMedia");
			// 
			// showToolStripMenuItem
			// 
			showToolStripMenuItem.Name = "showToolStripMenuItem";
			resources.ApplyResources(showToolStripMenuItem, "showToolStripMenuItem");
			showToolStripMenuItem.Click += MediaAnzeigenToolStripMenuItem_Click;
			// 
			// addToolStripMenuItem
			// 
			addToolStripMenuItem.Name = "addToolStripMenuItem";
			resources.ApplyResources(addToolStripMenuItem, "addToolStripMenuItem");
			addToolStripMenuItem.Click += MediaNeuToolStripMenuItem_Click;
			// 
			// deleteToolStripMenuItem
			// 
			resources.ApplyResources(deleteToolStripMenuItem, "deleteToolStripMenuItem");
			deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			deleteToolStripMenuItem.Click += MediaLöschenToolStripMenuItem_Click;
			// 
			// label6
			// 
			resources.ApplyResources(label6, "label6");
			label6.Name = "label6";
			// 
			// pictureBoxImgScreenshot
			// 
			pictureBoxImgScreenshot.BackColor = SystemColors.ControlLight;
			pictureBoxImgScreenshot.ContextMenuStrip = contextMenuStripMedia;
			resources.ApplyResources(pictureBoxImgScreenshot, "pictureBoxImgScreenshot");
			pictureBoxImgScreenshot.Name = "pictureBoxImgScreenshot";
			pictureBoxImgScreenshot.TabStop = false;
			pictureBoxImgScreenshot.Click += pictureBoxImgVideo_Click;
			// 
			// label7
			// 
			resources.ApplyResources(label7, "label7");
			label7.Name = "label7";
			// 
			// pictureBoxImgVideo
			// 
			pictureBoxImgVideo.BackColor = SystemColors.ControlLight;
			pictureBoxImgVideo.ContextMenuStrip = contextMenuStripMedia;
			resources.ApplyResources(pictureBoxImgVideo, "pictureBoxImgVideo");
			pictureBoxImgVideo.Name = "pictureBoxImgVideo";
			pictureBoxImgVideo.TabStop = false;
			pictureBoxImgVideo.Click += pictureBoxImgVideo_Click;
			// 
			// label8
			// 
			resources.ApplyResources(label8, "label8");
			label8.Name = "label8";
			// 
			// tableLayoutPanelRomDetails
			// 
			resources.ApplyResources(tableLayoutPanelRomDetails, "tableLayoutPanelRomDetails");
			tableLayoutPanelRomDetails.Controls.Add(textBoxRomDetailsAnzPlayer, 3, 2);
			tableLayoutPanelRomDetails.Controls.Add(label10, 2, 2);
			tableLayoutPanelRomDetails.Controls.Add(textBoxRomDetailsGenre, 1, 2);
			tableLayoutPanelRomDetails.Controls.Add(label9, 0, 2);
			tableLayoutPanelRomDetails.Controls.Add(textBoxRomDetailsPublisher, 3, 1);
			tableLayoutPanelRomDetails.Controls.Add(label5, 2, 1);
			tableLayoutPanelRomDetails.Controls.Add(textBoxRomDetailsDeveloper, 1, 1);
			tableLayoutPanelRomDetails.Controls.Add(label4, 0, 1);
			tableLayoutPanelRomDetails.Controls.Add(label3, 2, 0);
			tableLayoutPanelRomDetails.Controls.Add(label2, 0, 0);
			tableLayoutPanelRomDetails.Controls.Add(textBoxRomDetailsReleaseDate, 1, 0);
			tableLayoutPanelRomDetails.Controls.Add(buttonRomSave, 3, 3);
			tableLayoutPanelRomDetails.Controls.Add(buttonRomScrap, 1, 3);
			tableLayoutPanelRomDetails.Controls.Add(starRatingControlRom, 3, 0);
			tableLayoutPanelRomDetails.Name = "tableLayoutPanelRomDetails";
			// 
			// textBoxRomDetailsAnzPlayer
			// 
			resources.ApplyResources(textBoxRomDetailsAnzPlayer, "textBoxRomDetailsAnzPlayer");
			textBoxRomDetailsAnzPlayer.Name = "textBoxRomDetailsAnzPlayer";
			// 
			// label10
			// 
			resources.ApplyResources(label10, "label10");
			label10.Name = "label10";
			// 
			// textBoxRomDetailsGenre
			// 
			resources.ApplyResources(textBoxRomDetailsGenre, "textBoxRomDetailsGenre");
			textBoxRomDetailsGenre.Name = "textBoxRomDetailsGenre";
			// 
			// label9
			// 
			resources.ApplyResources(label9, "label9");
			label9.Name = "label9";
			// 
			// textBoxRomDetailsPublisher
			// 
			resources.ApplyResources(textBoxRomDetailsPublisher, "textBoxRomDetailsPublisher");
			textBoxRomDetailsPublisher.Name = "textBoxRomDetailsPublisher";
			// 
			// label5
			// 
			resources.ApplyResources(label5, "label5");
			label5.Name = "label5";
			// 
			// textBoxRomDetailsDeveloper
			// 
			resources.ApplyResources(textBoxRomDetailsDeveloper, "textBoxRomDetailsDeveloper");
			textBoxRomDetailsDeveloper.Name = "textBoxRomDetailsDeveloper";
			// 
			// label4
			// 
			resources.ApplyResources(label4, "label4");
			label4.Name = "label4";
			// 
			// label3
			// 
			resources.ApplyResources(label3, "label3");
			label3.Name = "label3";
			// 
			// label2
			// 
			resources.ApplyResources(label2, "label2");
			label2.Name = "label2";
			// 
			// textBoxRomDetailsReleaseDate
			// 
			resources.ApplyResources(textBoxRomDetailsReleaseDate, "textBoxRomDetailsReleaseDate");
			textBoxRomDetailsReleaseDate.Name = "textBoxRomDetailsReleaseDate";
			// 
			// buttonRomSave
			// 
			resources.ApplyResources(buttonRomSave, "buttonRomSave");
			buttonRomSave.Image = Properties.Resources.save_32;
			buttonRomSave.Name = "buttonRomSave";
			buttonRomSave.UseVisualStyleBackColor = true;
			buttonRomSave.Click += buttonRomSave_Click;
			// 
			// buttonRomScrap
			// 
			resources.ApplyResources(buttonRomScrap, "buttonRomScrap");
			buttonRomScrap.Image = Properties.Resources.joystick_32;
			buttonRomScrap.Name = "buttonRomScrap";
			buttonRomScrap.UseVisualStyleBackColor = true;
			buttonRomScrap.Click += buttonRomScrap_Click;
			// 
			// starRatingControlRom
			// 
			starRatingControlRom.AllowHalfStars = true;
			starRatingControlRom.EmptyColor = Color.LightGray;
			starRatingControlRom.FilledColor = Color.Gold;
			resources.ApplyResources(starRatingControlRom, "starRatingControlRom");
			starRatingControlRom.Name = "starRatingControlRom";
			starRatingControlRom.OutlineColor = Color.Black;
			starRatingControlRom.Rating = 0D;
			starRatingControlRom.StarCount = 5;
			starRatingControlRom.StarSpacing = 4;
			// 
			// statusStripMain
			// 
			statusStripMain.ImageScalingSize = new Size(20, 20);
			statusStripMain.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelMain });
			resources.ApplyResources(statusStripMain, "statusStripMain");
			statusStripMain.Name = "statusStripMain";
			// 
			// toolStripStatusLabelMain
			// 
			toolStripStatusLabelMain.Name = "toolStripStatusLabelMain";
			resources.ApplyResources(toolStripStatusLabelMain, "toolStripStatusLabelMain");
			// 
			// FormMain
			// 
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(splitContainerMain);
			Controls.Add(statusStripMain);
			Name = "FormMain";
			FormClosed += FormMain_FormClosed;
			Load += FormMain_Load;
			splitContainerMain.Panel1.ResumeLayout(false);
			splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
			splitContainerMain.ResumeLayout(false);
			tableLayoutPanelMainLeft.ResumeLayout(false);
			contextMenuStripSystems.ResumeLayout(false);
			tableLayoutPanelRomsControls.ResumeLayout(false);
			contextMenuStripRoms.ResumeLayout(false);
			splitContainerRight.Panel1.ResumeLayout(false);
			splitContainerRight.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainerRight).EndInit();
			splitContainerRight.ResumeLayout(false);
			splitContainerRightRom.Panel1.ResumeLayout(false);
			splitContainerRightRom.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainerRightRom).EndInit();
			splitContainerRightRom.ResumeLayout(false);
			tableLayoutPanelRightInnen.ResumeLayout(false);
			tableLayoutPanelRightInnen.PerformLayout();
			tableLayoutPanelSystem.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxRomSystem).EndInit();
			tableLayoutPanelRomMedia.ResumeLayout(false);
			tableLayoutPanelRomMedia.PerformLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxImgBox).EndInit();
			contextMenuStripMedia.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxImgScreenshot).EndInit();
			((System.ComponentModel.ISupportInitialize)pictureBoxImgVideo).EndInit();
			tableLayoutPanelRomDetails.ResumeLayout(false);
			tableLayoutPanelRomDetails.PerformLayout();
			statusStripMain.ResumeLayout(false);
			statusStripMain.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private StatusStrip statusStripMain;
		private SplitContainer splitContainerMain;
		private TableLayoutPanel tableLayoutPanelMainLeft;
		private ListView listViewRoms;
		private TableLayoutPanel tableLayoutPanelRomsControls;
		private Button buttonRomsRead;
		private ColumnHeader colRomsName;
		private Button buttonRomPath;
		private TableLayoutPanel tableLayoutPanelRightInnen;
		private TextBox textBoxRomName;
		private TextBox textBoxRomDesc;
		private TableLayoutPanel tableLayoutPanelRomMedia;
		private PictureBox pictureBoxImgBox;
		private PictureBox pictureBoxImgScreenshot;
		private PictureBox pictureBoxImgVideo;
		private TableLayoutPanel tableLayoutPanelRomDetails;
		private Label label3;
		private Label label2;
		private TextBox textBoxRomDetailsReleaseDate;
		private TextBox textBoxRomDetailsPublisher;
		private Label label5;
		private TextBox textBoxRomDetailsDeveloper;
		private Label label4;
		private Label label6;
		private Label label7;
		private Label label8;
		private Label label9;
		private TextBox textBoxRomDetailsAnzPlayer;
		private Label label10;
		private TextBox textBoxRomDetailsGenre;
		private Button buttonRomSave;
		private ListView listViewSystems;
		private ColumnHeader colSystemName;
		private ColumnHeader colSystemAnzRoms;
		private ColumnHeader colRomsRelease;
		private ColumnHeader colRomsGenre;
		private ColumnHeader colRomsAnzPlayer;
		private ColumnHeader colRomsRating;
		private ColumnHeader colSystemHersteller;
		private ColumnHeader colSystemVon;
		private ColumnHeader colSystemBis;
		private TableLayoutPanel tableLayoutPanelSystem;
		private PictureBox pictureBoxRomSystem;
		private ListBox listBoxSystem;
		private SplitContainer splitContainerRight;
		private Button buttonRomScrap;
		private ToolStripStatusLabel toolStripStatusLabelMain;
		private Button buttonOptions;
		private ColumnHeader colRomsFile;
		private SplitContainer splitContainerRightRom;
		private ContextMenuStrip contextMenuStripSystems;
		private ToolStripMenuItem alleRomsScrapenToolStripMenuItem;
		private ToolStripMenuItem detailsToolStripMenuItem;
		private ContextMenuStrip contextMenuStripRoms;
		private ToolStripMenuItem scrapToolStripMenuItem;
		private ToolStripMenuItem detailsToolStripMenuItem1;
		private ToolStripMenuItem löschenToolStripMenuItem;
		private ContextMenuStrip contextMenuStripMedia;
		private ToolStripMenuItem showToolStripMenuItem;
		private ToolStripMenuItem deleteToolStripMenuItem;
		private ToolStripMenuItem addToolStripMenuItem;
		private StarRatingControl starRatingControlRom;
	}
}
