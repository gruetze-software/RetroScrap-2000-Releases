namespace RetroScrap2000
{
	partial class FormUpdate
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUpdate));
			buttonOK = new Button();
			linkLabelDownload = new LinkLabel();
			labelUpdateText = new Label();
			SuspendLayout();
			// 
			// buttonOK
			// 
			buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonOK.DialogResult = DialogResult.OK;
			buttonOK.Location = new Point(321, 167);
			buttonOK.Name = "buttonOK";
			buttonOK.Size = new Size(94, 36);
			buttonOK.TabIndex = 1;
			buttonOK.Text = "OK";
			buttonOK.UseVisualStyleBackColor = true;
			// 
			// linkLabelDownload
			// 
			linkLabelDownload.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			linkLabelDownload.Location = new Point(39, 84);
			linkLabelDownload.Name = "linkLabelDownload";
			linkLabelDownload.Size = new Size(341, 54);
			linkLabelDownload.TabIndex = 2;
			linkLabelDownload.TabStop = true;
			linkLabelDownload.Text = "linkLabelDownload";
			// 
			// labelUpdateText
			// 
			labelUpdateText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			labelUpdateText.Location = new Point(12, 21);
			labelUpdateText.Name = "labelUpdateText";
			labelUpdateText.Size = new Size(407, 63);
			labelUpdateText.TabIndex = 3;
			labelUpdateText.Text = "A newer version is available. Update your old version {0} to the new version {1}. Use the Link for the download.";
			// 
			// FormUpdate
			// 
			AcceptButton = buttonOK;
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(427, 219);
			Controls.Add(labelUpdateText);
			Controls.Add(linkLabelDownload);
			Controls.Add(buttonOK);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormUpdate";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Update";
			Load += FormUpdate_Load;
			ResumeLayout(false);
		}

		#endregion
		private Button buttonOK;
		private LinkLabel linkLabelDownload;
		private Label labelUpdateText;
	}
}