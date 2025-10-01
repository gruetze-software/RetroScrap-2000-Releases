namespace RetroScrap2000
{
	partial class FormOptions
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOptions));
			buttonCancel = new Button();
			buttonOK = new Button();
			groupBox1 = new GroupBox();
			comboBoxLangRegion = new ComboBox();
			label8 = new Label();
			labelHelpLang = new Label();
			comboBoxLanguage = new ComboBox();
			groupBox2 = new GroupBox();
			listBoxApiTest = new ListBox();
			buttonApiTest = new Button();
			textBoxApiPwd = new TextBox();
			textBoxApiLogin = new TextBox();
			label2 = new Label();
			label1 = new Label();
			tabControlOptions = new TabControl();
			tabPageLang = new TabPage();
			groupBox4 = new GroupBox();
			linkLabelDonate = new LinkLabel();
			labelDonText = new Label();
			pictureBoxDonation = new PictureBox();
			groupBox3 = new GroupBox();
			label10 = new Label();
			buttonInfoRetroPie = new Button();
			radioButtonRetroArch = new RadioButton();
			radioButtonBatocera = new RadioButton();
			tabPageScrap = new TabPage();
			tabPageScrapData = new TabPage();
			groupBox5 = new GroupBox();
			checkBoxMediaMap = new CheckBox();
			checkBoxMediaManual = new CheckBox();
			checkBoxMediaVideo = new CheckBox();
			checkBoxMediaImageBox = new CheckBox();
			checkBoxMediaWheel = new CheckBox();
			checkBoxMediaScreenshot = new CheckBox();
			checkBoxMediaMarquee = new CheckBox();
			checkBoxMediaFanart = new CheckBox();
			tabPageAppInfo = new TabPage();
			tableLayoutPanelInfo = new TableLayoutPanel();
			tableLayoutPanelIcons = new TableLayoutPanel();
			pictureBoxScrap = new PictureBox();
			tableLayoutPanel1 = new TableLayoutPanel();
			pictureBoxAppIcon = new PictureBox();
			pictureBoxCompany = new PictureBox();
			tableLayoutPanelAppText = new TableLayoutPanel();
			linkLabel4 = new LinkLabel();
			linkLabel3 = new LinkLabel();
			linkLabel2 = new LinkLabel();
			label13 = new Label();
			label11 = new Label();
			label9 = new Label();
			label7 = new Label();
			labelInfoCopyright = new Label();
			label6 = new Label();
			labelInfoCompany = new Label();
			label5 = new Label();
			labelInfoVersion = new Label();
			label4 = new Label();
			label3 = new Label();
			labelInfoProduct = new Label();
			linkLabel1 = new LinkLabel();
			groupBox1.SuspendLayout();
			groupBox2.SuspendLayout();
			tabControlOptions.SuspendLayout();
			tabPageLang.SuspendLayout();
			groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxDonation).BeginInit();
			groupBox3.SuspendLayout();
			tabPageScrap.SuspendLayout();
			tabPageScrapData.SuspendLayout();
			groupBox5.SuspendLayout();
			tabPageAppInfo.SuspendLayout();
			tableLayoutPanelInfo.SuspendLayout();
			tableLayoutPanelIcons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxScrap).BeginInit();
			tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxAppIcon).BeginInit();
			((System.ComponentModel.ISupportInitialize)pictureBoxCompany).BeginInit();
			tableLayoutPanelAppText.SuspendLayout();
			SuspendLayout();
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
			// groupBox1
			// 
			groupBox1.Controls.Add(comboBoxLangRegion);
			groupBox1.Controls.Add(label8);
			groupBox1.Controls.Add(labelHelpLang);
			groupBox1.Controls.Add(comboBoxLanguage);
			resources.ApplyResources(groupBox1, "groupBox1");
			groupBox1.Name = "groupBox1";
			groupBox1.TabStop = false;
			// 
			// comboBoxLangRegion
			// 
			comboBoxLangRegion.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBoxLangRegion.FormattingEnabled = true;
			comboBoxLangRegion.Items.AddRange(new object[] { resources.GetString("comboBoxLangRegion.Items"), resources.GetString("comboBoxLangRegion.Items1"), resources.GetString("comboBoxLangRegion.Items2") });
			resources.ApplyResources(comboBoxLangRegion, "comboBoxLangRegion");
			comboBoxLangRegion.Name = "comboBoxLangRegion";
			// 
			// label8
			// 
			resources.ApplyResources(label8, "label8");
			label8.Name = "label8";
			// 
			// labelHelpLang
			// 
			resources.ApplyResources(labelHelpLang, "labelHelpLang");
			labelHelpLang.Name = "labelHelpLang";
			// 
			// comboBoxLanguage
			// 
			comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBoxLanguage.FormattingEnabled = true;
			resources.ApplyResources(comboBoxLanguage, "comboBoxLanguage");
			comboBoxLanguage.Name = "comboBoxLanguage";
			comboBoxLanguage.SelectedIndexChanged += comboBoxLanguage_SelectedIndexChanged;
			// 
			// groupBox2
			// 
			resources.ApplyResources(groupBox2, "groupBox2");
			groupBox2.Controls.Add(listBoxApiTest);
			groupBox2.Controls.Add(buttonApiTest);
			groupBox2.Controls.Add(textBoxApiPwd);
			groupBox2.Controls.Add(textBoxApiLogin);
			groupBox2.Controls.Add(label2);
			groupBox2.Controls.Add(label1);
			groupBox2.Name = "groupBox2";
			groupBox2.TabStop = false;
			// 
			// listBoxApiTest
			// 
			resources.ApplyResources(listBoxApiTest, "listBoxApiTest");
			listBoxApiTest.FormattingEnabled = true;
			listBoxApiTest.Name = "listBoxApiTest";
			// 
			// buttonApiTest
			// 
			resources.ApplyResources(buttonApiTest, "buttonApiTest");
			buttonApiTest.Name = "buttonApiTest";
			buttonApiTest.UseVisualStyleBackColor = true;
			buttonApiTest.Click += buttonApiTest_Click;
			// 
			// textBoxApiPwd
			// 
			resources.ApplyResources(textBoxApiPwd, "textBoxApiPwd");
			textBoxApiPwd.Name = "textBoxApiPwd";
			textBoxApiPwd.UseSystemPasswordChar = true;
			// 
			// textBoxApiLogin
			// 
			resources.ApplyResources(textBoxApiLogin, "textBoxApiLogin");
			textBoxApiLogin.Name = "textBoxApiLogin";
			// 
			// label2
			// 
			resources.ApplyResources(label2, "label2");
			label2.Name = "label2";
			// 
			// label1
			// 
			resources.ApplyResources(label1, "label1");
			label1.Name = "label1";
			// 
			// tabControlOptions
			// 
			resources.ApplyResources(tabControlOptions, "tabControlOptions");
			tabControlOptions.Controls.Add(tabPageLang);
			tabControlOptions.Controls.Add(tabPageScrap);
			tabControlOptions.Controls.Add(tabPageScrapData);
			tabControlOptions.Controls.Add(tabPageAppInfo);
			tabControlOptions.Name = "tabControlOptions";
			tabControlOptions.SelectedIndex = 0;
			// 
			// tabPageLang
			// 
			tabPageLang.Controls.Add(groupBox4);
			tabPageLang.Controls.Add(groupBox3);
			tabPageLang.Controls.Add(groupBox1);
			resources.ApplyResources(tabPageLang, "tabPageLang");
			tabPageLang.Name = "tabPageLang";
			tabPageLang.UseVisualStyleBackColor = true;
			// 
			// groupBox4
			// 
			resources.ApplyResources(groupBox4, "groupBox4");
			groupBox4.Controls.Add(linkLabelDonate);
			groupBox4.Controls.Add(labelDonText);
			groupBox4.Controls.Add(pictureBoxDonation);
			groupBox4.Name = "groupBox4";
			groupBox4.TabStop = false;
			// 
			// linkLabelDonate
			// 
			resources.ApplyResources(linkLabelDonate, "linkLabelDonate");
			linkLabelDonate.Name = "linkLabelDonate";
			linkLabelDonate.TabStop = true;
			// 
			// labelDonText
			// 
			resources.ApplyResources(labelDonText, "labelDonText");
			labelDonText.Name = "labelDonText";
			// 
			// pictureBoxDonation
			// 
			resources.ApplyResources(pictureBoxDonation, "pictureBoxDonation");
			pictureBoxDonation.Name = "pictureBoxDonation";
			pictureBoxDonation.TabStop = false;
			// 
			// groupBox3
			// 
			resources.ApplyResources(groupBox3, "groupBox3");
			groupBox3.Controls.Add(label10);
			groupBox3.Controls.Add(buttonInfoRetroPie);
			groupBox3.Controls.Add(radioButtonRetroArch);
			groupBox3.Controls.Add(radioButtonBatocera);
			groupBox3.Name = "groupBox3";
			groupBox3.TabStop = false;
			// 
			// label10
			// 
			resources.ApplyResources(label10, "label10");
			label10.Name = "label10";
			// 
			// buttonInfoRetroPie
			// 
			resources.ApplyResources(buttonInfoRetroPie, "buttonInfoRetroPie");
			buttonInfoRetroPie.Name = "buttonInfoRetroPie";
			buttonInfoRetroPie.UseVisualStyleBackColor = true;
			buttonInfoRetroPie.Click += buttonInfoRetroPie_Click;
			// 
			// radioButtonRetroArch
			// 
			resources.ApplyResources(radioButtonRetroArch, "radioButtonRetroArch");
			radioButtonRetroArch.Name = "radioButtonRetroArch";
			radioButtonRetroArch.TabStop = true;
			radioButtonRetroArch.UseVisualStyleBackColor = true;
			// 
			// radioButtonBatocera
			// 
			resources.ApplyResources(radioButtonBatocera, "radioButtonBatocera");
			radioButtonBatocera.Name = "radioButtonBatocera";
			radioButtonBatocera.TabStop = true;
			radioButtonBatocera.UseVisualStyleBackColor = true;
			// 
			// tabPageScrap
			// 
			tabPageScrap.Controls.Add(groupBox2);
			resources.ApplyResources(tabPageScrap, "tabPageScrap");
			tabPageScrap.Name = "tabPageScrap";
			tabPageScrap.UseVisualStyleBackColor = true;
			// 
			// tabPageScrapData
			// 
			tabPageScrapData.Controls.Add(groupBox5);
			resources.ApplyResources(tabPageScrapData, "tabPageScrapData");
			tabPageScrapData.Name = "tabPageScrapData";
			tabPageScrapData.UseVisualStyleBackColor = true;
			// 
			// groupBox5
			// 
			groupBox5.Controls.Add(checkBoxMediaMap);
			groupBox5.Controls.Add(checkBoxMediaManual);
			groupBox5.Controls.Add(checkBoxMediaVideo);
			groupBox5.Controls.Add(checkBoxMediaImageBox);
			groupBox5.Controls.Add(checkBoxMediaWheel);
			groupBox5.Controls.Add(checkBoxMediaScreenshot);
			groupBox5.Controls.Add(checkBoxMediaMarquee);
			groupBox5.Controls.Add(checkBoxMediaFanart);
			resources.ApplyResources(groupBox5, "groupBox5");
			groupBox5.Name = "groupBox5";
			groupBox5.TabStop = false;
			// 
			// checkBoxMediaMap
			// 
			resources.ApplyResources(checkBoxMediaMap, "checkBoxMediaMap");
			checkBoxMediaMap.Name = "checkBoxMediaMap";
			checkBoxMediaMap.UseVisualStyleBackColor = true;
			// 
			// checkBoxMediaManual
			// 
			resources.ApplyResources(checkBoxMediaManual, "checkBoxMediaManual");
			checkBoxMediaManual.Name = "checkBoxMediaManual";
			checkBoxMediaManual.UseVisualStyleBackColor = true;
			// 
			// checkBoxMediaVideo
			// 
			resources.ApplyResources(checkBoxMediaVideo, "checkBoxMediaVideo");
			checkBoxMediaVideo.Name = "checkBoxMediaVideo";
			checkBoxMediaVideo.UseVisualStyleBackColor = true;
			// 
			// checkBoxMediaImageBox
			// 
			resources.ApplyResources(checkBoxMediaImageBox, "checkBoxMediaImageBox");
			checkBoxMediaImageBox.Name = "checkBoxMediaImageBox";
			checkBoxMediaImageBox.UseVisualStyleBackColor = true;
			// 
			// checkBoxMediaWheel
			// 
			resources.ApplyResources(checkBoxMediaWheel, "checkBoxMediaWheel");
			checkBoxMediaWheel.Name = "checkBoxMediaWheel";
			checkBoxMediaWheel.UseVisualStyleBackColor = true;
			// 
			// checkBoxMediaScreenshot
			// 
			resources.ApplyResources(checkBoxMediaScreenshot, "checkBoxMediaScreenshot");
			checkBoxMediaScreenshot.Name = "checkBoxMediaScreenshot";
			checkBoxMediaScreenshot.UseVisualStyleBackColor = true;
			// 
			// checkBoxMediaMarquee
			// 
			resources.ApplyResources(checkBoxMediaMarquee, "checkBoxMediaMarquee");
			checkBoxMediaMarquee.Name = "checkBoxMediaMarquee";
			checkBoxMediaMarquee.UseVisualStyleBackColor = true;
			// 
			// checkBoxMediaFanart
			// 
			resources.ApplyResources(checkBoxMediaFanart, "checkBoxMediaFanart");
			checkBoxMediaFanart.Name = "checkBoxMediaFanart";
			checkBoxMediaFanart.UseVisualStyleBackColor = true;
			// 
			// tabPageAppInfo
			// 
			tabPageAppInfo.Controls.Add(tableLayoutPanelInfo);
			resources.ApplyResources(tabPageAppInfo, "tabPageAppInfo");
			tabPageAppInfo.Name = "tabPageAppInfo";
			tabPageAppInfo.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanelInfo
			// 
			resources.ApplyResources(tableLayoutPanelInfo, "tableLayoutPanelInfo");
			tableLayoutPanelInfo.Controls.Add(tableLayoutPanelIcons, 0, 0);
			tableLayoutPanelInfo.Controls.Add(tableLayoutPanelAppText, 1, 0);
			tableLayoutPanelInfo.Name = "tableLayoutPanelInfo";
			// 
			// tableLayoutPanelIcons
			// 
			resources.ApplyResources(tableLayoutPanelIcons, "tableLayoutPanelIcons");
			tableLayoutPanelIcons.Controls.Add(pictureBoxScrap, 0, 1);
			tableLayoutPanelIcons.Controls.Add(tableLayoutPanel1, 0, 0);
			tableLayoutPanelIcons.Name = "tableLayoutPanelIcons";
			// 
			// pictureBoxScrap
			// 
			pictureBoxScrap.Cursor = Cursors.Hand;
			resources.ApplyResources(pictureBoxScrap, "pictureBoxScrap");
			pictureBoxScrap.Name = "pictureBoxScrap";
			pictureBoxScrap.TabStop = false;
			pictureBoxScrap.Click += pictureBoxScrap_Click;
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(pictureBoxAppIcon, 0, 0);
			tableLayoutPanel1.Controls.Add(pictureBoxCompany, 1, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// pictureBoxAppIcon
			// 
			pictureBoxAppIcon.Cursor = Cursors.Hand;
			resources.ApplyResources(pictureBoxAppIcon, "pictureBoxAppIcon");
			pictureBoxAppIcon.Name = "pictureBoxAppIcon";
			pictureBoxAppIcon.TabStop = false;
			pictureBoxAppIcon.Click += pictureBoxAppIcon_Click;
			// 
			// pictureBoxCompany
			// 
			pictureBoxCompany.Cursor = Cursors.Hand;
			resources.ApplyResources(pictureBoxCompany, "pictureBoxCompany");
			pictureBoxCompany.Name = "pictureBoxCompany";
			pictureBoxCompany.TabStop = false;
			pictureBoxCompany.Click += pictureBoxCompany_Click;
			// 
			// tableLayoutPanelAppText
			// 
			resources.ApplyResources(tableLayoutPanelAppText, "tableLayoutPanelAppText");
			tableLayoutPanelAppText.Controls.Add(linkLabel4, 1, 7);
			tableLayoutPanelAppText.Controls.Add(linkLabel3, 1, 6);
			tableLayoutPanelAppText.Controls.Add(linkLabel2, 1, 5);
			tableLayoutPanelAppText.Controls.Add(label13, 0, 7);
			tableLayoutPanelAppText.Controls.Add(label11, 0, 6);
			tableLayoutPanelAppText.Controls.Add(label9, 0, 5);
			tableLayoutPanelAppText.Controls.Add(label7, 0, 4);
			tableLayoutPanelAppText.Controls.Add(labelInfoCopyright, 1, 3);
			tableLayoutPanelAppText.Controls.Add(label6, 0, 3);
			tableLayoutPanelAppText.Controls.Add(labelInfoCompany, 1, 2);
			tableLayoutPanelAppText.Controls.Add(label5, 0, 2);
			tableLayoutPanelAppText.Controls.Add(labelInfoVersion, 1, 1);
			tableLayoutPanelAppText.Controls.Add(label4, 0, 1);
			tableLayoutPanelAppText.Controls.Add(label3, 0, 0);
			tableLayoutPanelAppText.Controls.Add(labelInfoProduct, 1, 0);
			tableLayoutPanelAppText.Controls.Add(linkLabel1, 1, 4);
			tableLayoutPanelAppText.Name = "tableLayoutPanelAppText";
			// 
			// linkLabel4
			// 
			resources.ApplyResources(linkLabel4, "linkLabel4");
			linkLabel4.Name = "linkLabel4";
			linkLabel4.TabStop = true;
			// 
			// linkLabel3
			// 
			resources.ApplyResources(linkLabel3, "linkLabel3");
			linkLabel3.Name = "linkLabel3";
			linkLabel3.TabStop = true;
			// 
			// linkLabel2
			// 
			resources.ApplyResources(linkLabel2, "linkLabel2");
			linkLabel2.Name = "linkLabel2";
			linkLabel2.TabStop = true;
			// 
			// label13
			// 
			resources.ApplyResources(label13, "label13");
			label13.Name = "label13";
			// 
			// label11
			// 
			resources.ApplyResources(label11, "label11");
			label11.Name = "label11";
			// 
			// label9
			// 
			resources.ApplyResources(label9, "label9");
			label9.Name = "label9";
			// 
			// label7
			// 
			resources.ApplyResources(label7, "label7");
			label7.Name = "label7";
			// 
			// labelInfoCopyright
			// 
			resources.ApplyResources(labelInfoCopyright, "labelInfoCopyright");
			labelInfoCopyright.Name = "labelInfoCopyright";
			// 
			// label6
			// 
			resources.ApplyResources(label6, "label6");
			label6.Name = "label6";
			// 
			// labelInfoCompany
			// 
			resources.ApplyResources(labelInfoCompany, "labelInfoCompany");
			labelInfoCompany.Name = "labelInfoCompany";
			// 
			// label5
			// 
			resources.ApplyResources(label5, "label5");
			label5.Name = "label5";
			// 
			// labelInfoVersion
			// 
			resources.ApplyResources(labelInfoVersion, "labelInfoVersion");
			labelInfoVersion.Name = "labelInfoVersion";
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
			// labelInfoProduct
			// 
			resources.ApplyResources(labelInfoProduct, "labelInfoProduct");
			labelInfoProduct.Name = "labelInfoProduct";
			// 
			// linkLabel1
			// 
			resources.ApplyResources(linkLabel1, "linkLabel1");
			linkLabel1.Name = "linkLabel1";
			linkLabel1.TabStop = true;
			// 
			// FormOptions
			// 
			AcceptButton = buttonOK;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = buttonCancel;
			Controls.Add(tabControlOptions);
			Controls.Add(buttonOK);
			Controls.Add(buttonCancel);
			Name = "FormOptions";
			Load += FormOptions_Load;
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			tabControlOptions.ResumeLayout(false);
			tabPageLang.ResumeLayout(false);
			groupBox4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxDonation).EndInit();
			groupBox3.ResumeLayout(false);
			groupBox3.PerformLayout();
			tabPageScrap.ResumeLayout(false);
			tabPageScrapData.ResumeLayout(false);
			groupBox5.ResumeLayout(false);
			groupBox5.PerformLayout();
			tabPageAppInfo.ResumeLayout(false);
			tableLayoutPanelInfo.ResumeLayout(false);
			tableLayoutPanelIcons.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxScrap).EndInit();
			tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxAppIcon).EndInit();
			((System.ComponentModel.ISupportInitialize)pictureBoxCompany).EndInit();
			tableLayoutPanelAppText.ResumeLayout(false);
			tableLayoutPanelAppText.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Button buttonCancel;
		private Button buttonOK;
		private GroupBox groupBox1;
		private ComboBox comboBoxLanguage;
		private Label labelHelpLang;
		private GroupBox groupBox2;
		private Label label2;
		private Label label1;
		private Button buttonApiTest;
		private TextBox textBoxApiPwd;
		private TextBox textBoxApiLogin;
		private ListBox listBoxApiTest;
		private TabControl tabControlOptions;
		private TabPage tabPageLang;
		private TabPage tabPageScrap;
		private TabPage tabPageAppInfo;
		private TableLayoutPanel tableLayoutPanelInfo;
		private TableLayoutPanel tableLayoutPanelAppText;
		private PictureBox pictureBoxCompany;
		private PictureBox pictureBoxAppIcon;
		private Label labelInfoCopyright;
		private Label label6;
		private Label labelInfoCompany;
		private Label label5;
		private Label labelInfoVersion;
		private Label label4;
		private Label label3;
		private Label labelInfoProduct;
		private TableLayoutPanel tableLayoutPanelIcons;
		private PictureBox pictureBoxScrap;
		private TableLayoutPanel tableLayoutPanel1;
		private Label label13;
		private Label label11;
		private Label label9;
		private Label label7;
		private Label labelDonText;
		private GroupBox groupBox3;
		private RadioButton radioButtonRetroArch;
		private RadioButton radioButtonBatocera;
		private Button buttonInfoRetroPie;
		private Label label10;
		private LinkLabel linkLabel4;
		private LinkLabel linkLabel3;
		private LinkLabel linkLabel2;
		private LinkLabel linkLabel1;
		private GroupBox groupBox4;
		private PictureBox pictureBoxDonation;
		private LinkLabel linkLabelDonate;
		private TabPage tabPageScrapData;
		private GroupBox groupBox5;
		private CheckBox checkBoxMediaWheel;
		private CheckBox checkBoxMediaScreenshot;
		private CheckBox checkBoxMediaMarquee;
		private CheckBox checkBoxMediaFanart;
		private CheckBox checkBoxMediaImageBox;
		private CheckBox checkBoxMediaVideo;
		private CheckBox checkBoxMediaMap;
		private CheckBox checkBoxMediaManual;
		private ComboBox comboBoxLangRegion;
		private Label label8;
	}
}