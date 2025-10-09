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
			SystemAlleRomsScrapenToolStripMenuItem = new ToolStripMenuItem();
			SystemDetailsToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator3 = new ToolStripSeparator();
			SystemCleanToolStripMenuItem = new ToolStripMenuItem();
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
			RomscrapToolStripMenuItem = new ToolStripMenuItem();
			RomDetailsToolStripMenuItem = new ToolStripMenuItem();
			RomfavoriteToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			RomDeleteToolStripMenuItem = new ToolStripMenuItem();
			splitContainerRight = new SplitContainer();
			splitContainerRightRom = new SplitContainer();
			tableLayoutPanelRightInnen = new TableLayoutPanel();
			textBoxRomName = new TextBox();
			tableLayoutPanelSystem = new TableLayoutPanel();
			pictureBoxRomSystem = new PictureBox();
			listBoxSystem = new ListBox();
			textBoxRomDesc = new TextBox();
			panelMedia = new Panel();
			flowLayoutPanelMedia = new FlowLayoutPanel();
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
			statusStripMain = new StatusStrip();
			toolStripStatusLabelMain = new ToolStripStatusLabel();
			contextMenuStripMedia = new ContextMenuStrip(components);
			MediaviewToolStripMenuItem = new ToolStripMenuItem();
			MedianewToolStripMenuItem = new ToolStripMenuItem();
			MediadeleteToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			addType1ToolStripMenuItem = new ToolStripMenuItem();
			addType2ToolStripMenuItem = new ToolStripMenuItem();
			addType3ToolStripMenuItem = new ToolStripMenuItem();
			addType4ToolStripMenuItem = new ToolStripMenuItem();
			addType5ToolStripMenuItem = new ToolStripMenuItem();
			addType6ToolStripMenuItem = new ToolStripMenuItem();
			addType7ToolStripMenuItem = new ToolStripMenuItem();
			addType8ToolStripMenuItem = new ToolStripMenuItem();
			addType9ToolStripMenuItem = new ToolStripMenuItem();
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
			panelMedia.SuspendLayout();
			tableLayoutPanelRomDetails.SuspendLayout();
			statusStripMain.SuspendLayout();
			contextMenuStripMedia.SuspendLayout();
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
			contextMenuStripSystems.Items.AddRange(new ToolStripItem[] { SystemAlleRomsScrapenToolStripMenuItem, SystemDetailsToolStripMenuItem, toolStripSeparator3, SystemCleanToolStripMenuItem });
			contextMenuStripSystems.Name = "contextMenuStripSystems";
			resources.ApplyResources(contextMenuStripSystems, "contextMenuStripSystems");
			// 
			// SystemAlleRomsScrapenToolStripMenuItem
			// 
			SystemAlleRomsScrapenToolStripMenuItem.Name = "SystemAlleRomsScrapenToolStripMenuItem";
			resources.ApplyResources(SystemAlleRomsScrapenToolStripMenuItem, "SystemAlleRomsScrapenToolStripMenuItem");
			SystemAlleRomsScrapenToolStripMenuItem.Click += SystemAllRomsScrapToolStripMenuItem_Click;
			// 
			// SystemDetailsToolStripMenuItem
			// 
			SystemDetailsToolStripMenuItem.Name = "SystemDetailsToolStripMenuItem";
			resources.ApplyResources(SystemDetailsToolStripMenuItem, "SystemDetailsToolStripMenuItem");
			SystemDetailsToolStripMenuItem.Click += SystemDetailsToolStripMenuItem_Click;
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";
			resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
			// 
			// SystemCleanToolStripMenuItem
			// 
			SystemCleanToolStripMenuItem.Name = "SystemCleanToolStripMenuItem";
			resources.ApplyResources(SystemCleanToolStripMenuItem, "SystemCleanToolStripMenuItem");
			SystemCleanToolStripMenuItem.Click += cleanToolStripMenuItem_Click;
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
			contextMenuStripRoms.Items.AddRange(new ToolStripItem[] { RomscrapToolStripMenuItem, RomDetailsToolStripMenuItem, RomfavoriteToolStripMenuItem, toolStripSeparator2, RomDeleteToolStripMenuItem });
			contextMenuStripRoms.Name = "contextMenuStripRoms";
			resources.ApplyResources(contextMenuStripRoms, "contextMenuStripRoms");
			// 
			// RomscrapToolStripMenuItem
			// 
			RomscrapToolStripMenuItem.Name = "RomscrapToolStripMenuItem";
			resources.ApplyResources(RomscrapToolStripMenuItem, "RomscrapToolStripMenuItem");
			RomscrapToolStripMenuItem.Click += RomScrapToolStripMenuItem_Click;
			// 
			// RomDetailsToolStripMenuItem
			// 
			RomDetailsToolStripMenuItem.Name = "RomDetailsToolStripMenuItem";
			resources.ApplyResources(RomDetailsToolStripMenuItem, "RomDetailsToolStripMenuItem");
			RomDetailsToolStripMenuItem.Click += RomDetailsToolStripMenuItem_Click;
			// 
			// RomfavoriteToolStripMenuItem
			// 
			RomfavoriteToolStripMenuItem.Name = "RomfavoriteToolStripMenuItem";
			resources.ApplyResources(RomfavoriteToolStripMenuItem, "RomfavoriteToolStripMenuItem");
			RomfavoriteToolStripMenuItem.Click += RomfavoriteToolStripMenuItem_Click;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
			// 
			// RomDeleteToolStripMenuItem
			// 
			RomDeleteToolStripMenuItem.Name = "RomDeleteToolStripMenuItem";
			resources.ApplyResources(RomDeleteToolStripMenuItem, "RomDeleteToolStripMenuItem");
			RomDeleteToolStripMenuItem.Click += RomLöschenToolStripMenuItem_Click;
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
			splitContainerRightRom.Panel2.Controls.Add(panelMedia);
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
			// panelMedia
			// 
			resources.ApplyResources(panelMedia, "panelMedia");
			panelMedia.Controls.Add(flowLayoutPanelMedia);
			panelMedia.Name = "panelMedia";
			// 
			// flowLayoutPanelMedia
			// 
			resources.ApplyResources(flowLayoutPanelMedia, "flowLayoutPanelMedia");
			flowLayoutPanelMedia.Name = "flowLayoutPanelMedia";
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
			// contextMenuStripMedia
			// 
			contextMenuStripMedia.ImageScalingSize = new Size(20, 20);
			contextMenuStripMedia.Items.AddRange(new ToolStripItem[] { MediaviewToolStripMenuItem, MedianewToolStripMenuItem, MediadeleteToolStripMenuItem, toolStripSeparator1, addType1ToolStripMenuItem, addType2ToolStripMenuItem, addType3ToolStripMenuItem, addType4ToolStripMenuItem, addType5ToolStripMenuItem, addType6ToolStripMenuItem, addType7ToolStripMenuItem, addType8ToolStripMenuItem, addType9ToolStripMenuItem });
			contextMenuStripMedia.Name = "contextMenuStripMedia";
			resources.ApplyResources(contextMenuStripMedia, "contextMenuStripMedia");
			// 
			// MediaviewToolStripMenuItem
			// 
			MediaviewToolStripMenuItem.Name = "MediaviewToolStripMenuItem";
			resources.ApplyResources(MediaviewToolStripMenuItem, "MediaviewToolStripMenuItem");
			MediaviewToolStripMenuItem.Click += viewToolStripMenuItem_Click;
			// 
			// MedianewToolStripMenuItem
			// 
			MedianewToolStripMenuItem.Name = "MedianewToolStripMenuItem";
			resources.ApplyResources(MedianewToolStripMenuItem, "MedianewToolStripMenuItem");
			MedianewToolStripMenuItem.Click += newToolStripMenuItem_Click;
			// 
			// MediadeleteToolStripMenuItem
			// 
			MediadeleteToolStripMenuItem.Name = "MediadeleteToolStripMenuItem";
			resources.ApplyResources(MediadeleteToolStripMenuItem, "MediadeleteToolStripMenuItem");
			MediadeleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
			// 
			// addType1ToolStripMenuItem
			// 
			addType1ToolStripMenuItem.Name = "addType1ToolStripMenuItem";
			resources.ApplyResources(addType1ToolStripMenuItem, "addType1ToolStripMenuItem");
			addType1ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType2ToolStripMenuItem
			// 
			addType2ToolStripMenuItem.Name = "addType2ToolStripMenuItem";
			resources.ApplyResources(addType2ToolStripMenuItem, "addType2ToolStripMenuItem");
			addType2ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType3ToolStripMenuItem
			// 
			addType3ToolStripMenuItem.Name = "addType3ToolStripMenuItem";
			resources.ApplyResources(addType3ToolStripMenuItem, "addType3ToolStripMenuItem");
			addType3ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType4ToolStripMenuItem
			// 
			addType4ToolStripMenuItem.Name = "addType4ToolStripMenuItem";
			resources.ApplyResources(addType4ToolStripMenuItem, "addType4ToolStripMenuItem");
			addType4ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType5ToolStripMenuItem
			// 
			addType5ToolStripMenuItem.Name = "addType5ToolStripMenuItem";
			resources.ApplyResources(addType5ToolStripMenuItem, "addType5ToolStripMenuItem");
			addType5ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType6ToolStripMenuItem
			// 
			addType6ToolStripMenuItem.Name = "addType6ToolStripMenuItem";
			resources.ApplyResources(addType6ToolStripMenuItem, "addType6ToolStripMenuItem");
			addType6ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType7ToolStripMenuItem
			// 
			addType7ToolStripMenuItem.Name = "addType7ToolStripMenuItem";
			resources.ApplyResources(addType7ToolStripMenuItem, "addType7ToolStripMenuItem");
			addType7ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType8ToolStripMenuItem
			// 
			addType8ToolStripMenuItem.Name = "addType8ToolStripMenuItem";
			resources.ApplyResources(addType8ToolStripMenuItem, "addType8ToolStripMenuItem");
			addType8ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
			// 
			// addType9ToolStripMenuItem
			// 
			addType9ToolStripMenuItem.Name = "addType9ToolStripMenuItem";
			resources.ApplyResources(addType9ToolStripMenuItem, "addType9ToolStripMenuItem");
			addType9ToolStripMenuItem.Click += addMediaToolStripMenuItem_Click;
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
			panelMedia.ResumeLayout(false);
			tableLayoutPanelRomDetails.ResumeLayout(false);
			tableLayoutPanelRomDetails.PerformLayout();
			statusStripMain.ResumeLayout(false);
			statusStripMain.PerformLayout();
			contextMenuStripMedia.ResumeLayout(false);
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
		private TableLayoutPanel tableLayoutPanelRomDetails;
		private Label label3;
		private Label label2;
		private TextBox textBoxRomDetailsReleaseDate;
		private TextBox textBoxRomDetailsPublisher;
		private Label label5;
		private TextBox textBoxRomDetailsDeveloper;
		private Label label4;
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
		private ToolStripMenuItem SystemAlleRomsScrapenToolStripMenuItem;
		private ToolStripMenuItem SystemDetailsToolStripMenuItem;
		private ContextMenuStrip contextMenuStripRoms;
		private ToolStripMenuItem RomscrapToolStripMenuItem;
		private ToolStripMenuItem RomDetailsToolStripMenuItem;
		private ToolStripMenuItem RomDeleteToolStripMenuItem;
		private Panel panelMedia;
		private FlowLayoutPanel flowLayoutPanelMedia;
		private ContextMenuStrip contextMenuStripMedia;
		private ToolStripMenuItem MediaviewToolStripMenuItem;
		private ToolStripMenuItem MedianewToolStripMenuItem;
		private ToolStripMenuItem MediadeleteToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem addType1ToolStripMenuItem;
		private ToolStripMenuItem addType2ToolStripMenuItem;
		private ToolStripMenuItem addType3ToolStripMenuItem;
		private ToolStripMenuItem addType4ToolStripMenuItem;
		private ToolStripMenuItem addType5ToolStripMenuItem;
		private ToolStripMenuItem addType6ToolStripMenuItem;
		private ToolStripMenuItem addType7ToolStripMenuItem;
		private ToolStripMenuItem addType8ToolStripMenuItem;
		private ToolStripMenuItem addType9ToolStripMenuItem;
		private ToolStripMenuItem RomfavoriteToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem SystemCleanToolStripMenuItem;
	}
}
