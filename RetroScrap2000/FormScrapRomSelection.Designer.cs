namespace RetroScrap2000
{
	partial class FormScrapRomSelection
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormScrapRomSelection));
			groupBox1 = new GroupBox();
			listViewData = new ListView();
			colName = new ColumnHeader();
			colGenre = new ColumnHeader();
			colDeveloper = new ColumnHeader();
			colManufacturer = new ColumnHeader();
			colDesc = new ColumnHeader();
			buttonCancel = new Button();
			buttonOK = new Button();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// groupBox1
			// 
			groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBox1.Controls.Add(listViewData);
			groupBox1.Location = new Point(12, 12);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(492, 206);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			groupBox1.Text = "Hit list";
			// 
			// listViewData
			// 
			listViewData.Columns.AddRange(new ColumnHeader[] { colName, colGenre, colDeveloper, colManufacturer, colDesc });
			listViewData.Dock = DockStyle.Fill;
			listViewData.FullRowSelect = true;
			listViewData.GridLines = true;
			listViewData.HideSelection = true;
			listViewData.Location = new Point(3, 19);
			listViewData.MultiSelect = false;
			listViewData.Name = "listViewData";
			listViewData.Size = new Size(486, 184);
			listViewData.TabIndex = 0;
			listViewData.UseCompatibleStateImageBehavior = false;
			listViewData.View = View.Details;
			listViewData.SelectedIndexChanged += listViewData_SelectedIndexChanged;
			// 
			// colName
			// 
			colName.Text = "Name";
			colName.Width = 120;
			// 
			// colGenre
			// 
			colGenre.Text = "Genre";
			colGenre.Width = 90;
			// 
			// colDeveloper
			// 
			colDeveloper.Text = "Developer";
			colDeveloper.Width = 90;
			// 
			// colManufacturer
			// 
			colManufacturer.Text = "Manufacturer";
			colManufacturer.Width = 90;
			// 
			// colDesc
			// 
			colDesc.Text = "Description";
			colDesc.Width = 300;
			// 
			// buttonCancel
			// 
			buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonCancel.DialogResult = DialogResult.Cancel;
			buttonCancel.Location = new Point(429, 224);
			buttonCancel.Name = "buttonCancel";
			buttonCancel.Size = new Size(75, 29);
			buttonCancel.TabIndex = 1;
			buttonCancel.Text = "Cancel";
			buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonOK.DialogResult = DialogResult.OK;
			buttonOK.Location = new Point(348, 224);
			buttonOK.Name = "buttonOK";
			buttonOK.Size = new Size(75, 29);
			buttonOK.TabIndex = 2;
			buttonOK.Text = "Apply";
			buttonOK.UseVisualStyleBackColor = true;
			buttonOK.Click += buttonOK_Click;
			// 
			// FormScrapRomSelection
			// 
			AcceptButton = buttonCancel;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = buttonCancel;
			ClientSize = new Size(516, 265);
			Controls.Add(buttonOK);
			Controls.Add(buttonCancel);
			Controls.Add(groupBox1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormScrapRomSelection";
			SizeGripStyle = SizeGripStyle.Show;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Selection";
			FormClosing += FormScrapRomSelection_FormClosing;
			Load += FormScrapRomSelection_Load;
			groupBox1.ResumeLayout(false);
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
	}
}