namespace RetroScrap2000
{
	partial class FormScrapRomResearch
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormScrapRomResearch));
			groupBox1 = new GroupBox();
			listViewData = new ListView();
			colName = new ColumnHeader();
			colGenre = new ColumnHeader();
			colDeveloper = new ColumnHeader();
			colManufacturer = new ColumnHeader();
			colDesc = new ColumnHeader();
			buttonCancel = new Button();
			buttonOK = new Button();
			groupBox2 = new GroupBox();
			pictureBoxAniJoystick = new PictureBox();
			buttonStartStop = new Button();
			textBoxSearchName = new TextBox();
			label1 = new Label();
			groupBox1.SuspendLayout();
			groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxAniJoystick).BeginInit();
			SuspendLayout();
			// 
			// groupBox1
			// 
			resources.ApplyResources(groupBox1, "groupBox1");
			groupBox1.Controls.Add(listViewData);
			groupBox1.Name = "groupBox1";
			groupBox1.TabStop = false;
			// 
			// listViewData
			// 
			resources.ApplyResources(listViewData, "listViewData");
			listViewData.Columns.AddRange(new ColumnHeader[] { colName, colGenre, colDeveloper, colManufacturer, colDesc });
			listViewData.FullRowSelect = true;
			listViewData.GridLines = true;
			listViewData.HideSelection = true;
			listViewData.MultiSelect = false;
			listViewData.Name = "listViewData";
			listViewData.UseCompatibleStateImageBehavior = false;
			listViewData.View = View.Details;
			listViewData.SelectedIndexChanged += listViewData_SelectedIndexChanged;
			// 
			// colName
			// 
			resources.ApplyResources(colName, "colName");
			// 
			// colGenre
			// 
			resources.ApplyResources(colGenre, "colGenre");
			// 
			// colDeveloper
			// 
			resources.ApplyResources(colDeveloper, "colDeveloper");
			// 
			// colManufacturer
			// 
			resources.ApplyResources(colManufacturer, "colManufacturer");
			// 
			// colDesc
			// 
			resources.ApplyResources(colDesc, "colDesc");
			// 
			// buttonCancel
			// 
			resources.ApplyResources(buttonCancel, "buttonCancel");
			buttonCancel.DialogResult = DialogResult.Cancel;
			buttonCancel.Name = "buttonCancel";
			buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			resources.ApplyResources(buttonOK, "buttonOK");
			buttonOK.DialogResult = DialogResult.OK;
			buttonOK.Name = "buttonOK";
			buttonOK.UseVisualStyleBackColor = true;
			buttonOK.Click += buttonOK_Click;
			// 
			// groupBox2
			// 
			resources.ApplyResources(groupBox2, "groupBox2");
			groupBox2.Controls.Add(pictureBoxAniJoystick);
			groupBox2.Controls.Add(buttonStartStop);
			groupBox2.Controls.Add(textBoxSearchName);
			groupBox2.Controls.Add(label1);
			groupBox2.Name = "groupBox2";
			groupBox2.TabStop = false;
			// 
			// pictureBoxAniJoystick
			// 
			resources.ApplyResources(pictureBoxAniJoystick, "pictureBoxAniJoystick");
			pictureBoxAniJoystick.Name = "pictureBoxAniJoystick";
			pictureBoxAniJoystick.TabStop = false;
			// 
			// buttonStartStop
			// 
			resources.ApplyResources(buttonStartStop, "buttonStartStop");
			buttonStartStop.Name = "buttonStartStop";
			buttonStartStop.UseVisualStyleBackColor = true;
			buttonStartStop.Click += buttonStartStop_Click;
			// 
			// textBoxSearchName
			// 
			resources.ApplyResources(textBoxSearchName, "textBoxSearchName");
			textBoxSearchName.Name = "textBoxSearchName";
			// 
			// label1
			// 
			resources.ApplyResources(label1, "label1");
			label1.Name = "label1";
			// 
			// FormScrapRomResearch
			// 
			AcceptButton = buttonCancel;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = buttonCancel;
			Controls.Add(groupBox2);
			Controls.Add(buttonOK);
			Controls.Add(buttonCancel);
			Controls.Add(groupBox1);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormScrapRomResearch";
			SizeGripStyle = SizeGripStyle.Show;
			FormClosing += FormScrapRomResearch_FormClosing;
			Load += FormScrapRomResearch_Load;
			groupBox1.ResumeLayout(false);
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxAniJoystick).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private GroupBox groupBox1;
		private ListView listViewData;
		private ColumnHeader colName;
		private ColumnHeader colGenre;
		private ColumnHeader colDeveloper;
		private ColumnHeader colManufacturer;
		private ColumnHeader colDesc;
		private Button buttonCancel;
		private Button buttonOK;
		private GroupBox groupBox2;
		private Button buttonStartStop;
		private TextBox textBoxSearchName;
		private Label label1;
		private PictureBox pictureBoxAniJoystick;
	}
}