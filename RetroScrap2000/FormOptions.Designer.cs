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
			labelHelpLang = new Label();
			comboBoxLanguage = new ComboBox();
			groupBox2 = new GroupBox();
			listBoxApiTest = new ListBox();
			buttonApiTest = new Button();
			textBoxApiPwd = new TextBox();
			textBoxApiLogin = new TextBox();
			label2 = new Label();
			label1 = new Label();
			groupBox1.SuspendLayout();
			groupBox2.SuspendLayout();
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
			resources.ApplyResources(groupBox1, "groupBox1");
			groupBox1.Controls.Add(labelHelpLang);
			groupBox1.Controls.Add(comboBoxLanguage);
			groupBox1.Name = "groupBox1";
			groupBox1.TabStop = false;
			// 
			// labelHelpLang
			// 
			resources.ApplyResources(labelHelpLang, "labelHelpLang");
			labelHelpLang.Name = "labelHelpLang";
			// 
			// comboBoxLanguage
			// 
			resources.ApplyResources(comboBoxLanguage, "comboBoxLanguage");
			comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBoxLanguage.FormattingEnabled = true;
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
			// FormOptions
			// 
			AcceptButton = buttonOK;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = buttonCancel;
			Controls.Add(groupBox2);
			Controls.Add(groupBox1);
			Controls.Add(buttonOK);
			Controls.Add(buttonCancel);
			Name = "FormOptions";
			Load += FormOptions_Load;
			groupBox1.ResumeLayout(false);
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
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
	}
}