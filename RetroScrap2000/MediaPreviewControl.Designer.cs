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
			components = new System.ComponentModel.Container();
			tableLayoutPanelMain = new TableLayoutPanel();
			labelTitle = new Label();
			pictureBoxMedium = new PictureBox();
			panelForControls = new Panel();
			flowLayoutPanelButtons = new FlowLayoutPanel();
			buttonOpen = new Button();
			buttonNew = new Button();
			buttonDelete = new Button();
			checkBoxTakeOver = new CheckBox();
			toolTipButtons = new ToolTip(components);
			tableLayoutPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxMedium).BeginInit();
			panelForControls.SuspendLayout();
			flowLayoutPanelButtons.SuspendLayout();
			SuspendLayout();
			// 
			// tableLayoutPanelMain
			// 
			tableLayoutPanelMain.ColumnCount = 1;
			tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanelMain.Controls.Add(labelTitle, 0, 0);
			tableLayoutPanelMain.Controls.Add(pictureBoxMedium, 0, 1);
			tableLayoutPanelMain.Controls.Add(panelForControls, 0, 2);
			tableLayoutPanelMain.Dock = DockStyle.Fill;
			tableLayoutPanelMain.Location = new Point(6, 7);
			tableLayoutPanelMain.Margin = new Padding(3, 4, 3, 4);
			tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			tableLayoutPanelMain.RowCount = 3;
			tableLayoutPanelMain.RowStyles.Add(new RowStyle());
			tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanelMain.RowStyles.Add(new RowStyle());
			tableLayoutPanelMain.Size = new Size(171, 279);
			tableLayoutPanelMain.TabIndex = 0;
			// 
			// labelTitle
			// 
			labelTitle.AutoSize = true;
			labelTitle.Dock = DockStyle.Fill;
			labelTitle.Location = new Point(3, 0);
			labelTitle.Name = "labelTitle";
			labelTitle.Size = new Size(165, 20);
			labelTitle.TabIndex = 1;
			labelTitle.Text = "Media";
			labelTitle.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// pictureBoxMedium
			// 
			pictureBoxMedium.BackColor = SystemColors.Control;
			pictureBoxMedium.Dock = DockStyle.Fill;
			pictureBoxMedium.Location = new Point(3, 24);
			pictureBoxMedium.Margin = new Padding(3, 4, 3, 4);
			pictureBoxMedium.Name = "pictureBoxMedium";
			pictureBoxMedium.Size = new Size(165, 204);
			pictureBoxMedium.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBoxMedium.TabIndex = 1;
			pictureBoxMedium.TabStop = false;
			pictureBoxMedium.MouseClick += pictureBoxMedium_MouseClick;
			// 
			// panelForControls
			// 
			panelForControls.Anchor = AnchorStyles.None;
			panelForControls.Controls.Add(flowLayoutPanelButtons);
			panelForControls.Controls.Add(checkBoxTakeOver);
			panelForControls.Location = new Point(3, 236);
			panelForControls.Margin = new Padding(3, 4, 3, 4);
			panelForControls.Name = "panelForControls";
			panelForControls.Size = new Size(165, 39);
			panelForControls.TabIndex = 2;
			// 
			// flowLayoutPanelButtons
			// 
			flowLayoutPanelButtons.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			flowLayoutPanelButtons.AutoSize = true;
			flowLayoutPanelButtons.Controls.Add(buttonOpen);
			flowLayoutPanelButtons.Controls.Add(buttonNew);
			flowLayoutPanelButtons.Controls.Add(buttonDelete);
			flowLayoutPanelButtons.Location = new Point(29, 0);
			flowLayoutPanelButtons.Margin = new Padding(3, 4, 3, 4);
			flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
			flowLayoutPanelButtons.Size = new Size(111, 41);
			flowLayoutPanelButtons.TabIndex = 1;
			flowLayoutPanelButtons.WrapContents = false;
			// 
			// buttonOpen
			// 
			buttonOpen.Anchor = AnchorStyles.None;
			buttonOpen.Location = new Point(3, 4);
			buttonOpen.Margin = new Padding(3, 4, 3, 4);
			buttonOpen.Name = "buttonOpen";
			buttonOpen.Size = new Size(29, 33);
			buttonOpen.TabIndex = 0;
			buttonOpen.UseVisualStyleBackColor = true;
			buttonOpen.Click += buttonOpen_Click;
			// 
			// buttonNew
			// 
			buttonNew.Anchor = AnchorStyles.None;
			buttonNew.Location = new Point(38, 4);
			buttonNew.Margin = new Padding(3, 4, 3, 4);
			buttonNew.Name = "buttonNew";
			buttonNew.Size = new Size(29, 33);
			buttonNew.TabIndex = 1;
			buttonNew.UseVisualStyleBackColor = true;
			buttonNew.Click += buttonNew_Click;
			// 
			// buttonDelete
			// 
			buttonDelete.Anchor = AnchorStyles.None;
			buttonDelete.Location = new Point(73, 4);
			buttonDelete.Margin = new Padding(3, 4, 3, 4);
			buttonDelete.Name = "buttonDelete";
			buttonDelete.Size = new Size(29, 33);
			buttonDelete.TabIndex = 2;
			buttonDelete.UseVisualStyleBackColor = true;
			buttonDelete.Click += buttonDelete_Click;
			// 
			// checkBoxTakeOver
			// 
			checkBoxTakeOver.Anchor = AnchorStyles.None;
			checkBoxTakeOver.AutoSize = true;
			checkBoxTakeOver.Location = new Point(77, 11);
			checkBoxTakeOver.Margin = new Padding(3, 4, 3, 4);
			checkBoxTakeOver.Name = "checkBoxTakeOver";
			checkBoxTakeOver.Size = new Size(18, 17);
			checkBoxTakeOver.TabIndex = 0;
			checkBoxTakeOver.UseVisualStyleBackColor = true;
			// 
			// toolTipButtons
			// 
			toolTipButtons.StripAmpersands = true;
			// 
			// MediaPreviewControl
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.ControlLightLight;
			Controls.Add(tableLayoutPanelMain);
			Margin = new Padding(3, 4, 3, 4);
			Name = "MediaPreviewControl";
			Padding = new Padding(6, 7, 6, 7);
			Size = new Size(183, 293);
			tableLayoutPanelMain.ResumeLayout(false);
			tableLayoutPanelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxMedium).EndInit();
			panelForControls.ResumeLayout(false);
			panelForControls.PerformLayout();
			flowLayoutPanelButtons.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel tableLayoutPanelMain;
		private Label labelTitle;
		private PictureBox pictureBoxMedium;
		private Panel panelForControls;
		private FlowLayoutPanel flowLayoutPanelButtons;
		private Button buttonOpen;
		private CheckBox checkBoxTakeOver;
		private Button buttonNew;
		private Button buttonDelete;
		private ToolTip toolTipButtons;
	}
}
