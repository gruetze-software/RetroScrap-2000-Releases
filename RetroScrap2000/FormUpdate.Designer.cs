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
			buttonOK.Location = new Point(281, 125);
			buttonOK.Margin = new Padding(3, 2, 3, 2);
			buttonOK.Name = "buttonOK";
			buttonOK.Size = new Size(82, 27);
			buttonOK.TabIndex = 1;
			buttonOK.Text = "OK";
			buttonOK.UseVisualStyleBackColor = true;
			// 
			// linkLabelDownload
			// 
			linkLabelDownload.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			linkLabelDownload.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
			linkLabelDownload.Location = new Point(12, 51);
			linkLabelDownload.Name = "linkLabelDownload";
			linkLabelDownload.Size = new Size(350, 61);
			linkLabelDownload.TabIndex = 2;
			linkLabelDownload.TabStop = true;
			linkLabelDownload.Text = "https://bit.ly/RetroScrap2000-Latest";
			linkLabelDownload.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// labelUpdateText
			// 
			labelUpdateText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			labelUpdateText.Location = new Point(10, 9);
			labelUpdateText.Name = "labelUpdateText";
			labelUpdateText.Size = new Size(356, 42);
			labelUpdateText.TabIndex = 3;
			labelUpdateText.Text = "A newer version is available. Update your old version {0} to the new version {1}. Use the Link for the download.";
			labelUpdateText.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// FormUpdate
			// 
			AcceptButton = buttonOK;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(374, 164);
			Controls.Add(labelUpdateText);
			Controls.Add(linkLabelDownload);
			Controls.Add(buttonOK);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(3, 2, 3, 2);
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