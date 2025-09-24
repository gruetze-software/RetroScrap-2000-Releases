namespace RetroScrap2000
{
	partial class FormRomDetails
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
			buttonOK = new Button();
			listViewDetails = new ListView();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			SuspendLayout();
			// 
			// buttonOK
			// 
			buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonOK.DialogResult = DialogResult.OK;
			buttonOK.Location = new Point(544, 250);
			buttonOK.Name = "buttonOK";
			buttonOK.Size = new Size(94, 29);
			buttonOK.TabIndex = 0;
			buttonOK.Text = "OK";
			buttonOK.UseVisualStyleBackColor = true;
			// 
			// listViewDetails
			// 
			listViewDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			listViewDetails.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
			listViewDetails.FullRowSelect = true;
			listViewDetails.GridLines = true;
			listViewDetails.Location = new Point(12, 12);
			listViewDetails.Name = "listViewDetails";
			listViewDetails.Size = new Size(626, 230);
			listViewDetails.TabIndex = 1;
			listViewDetails.UseCompatibleStateImageBehavior = false;
			listViewDetails.View = View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Key";
			columnHeader1.Width = 90;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Value";
			columnHeader2.Width = 250;
			// 
			// FormRomDetails
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(650, 291);
			Controls.Add(listViewDetails);
			Controls.Add(buttonOK);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormRomDetails";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Details";
			Load += FormRomDetails_Load;
			Resize += FormRomDetails_Resize;
			ResumeLayout(false);
		}

		#endregion

		private Button buttonOK;
		private ListView listViewDetails;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
	}
}