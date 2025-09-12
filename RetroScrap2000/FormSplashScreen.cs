using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RetroScrap2000
{
	public partial class FormSplashScreen : Form
	{
		public FormSplashScreen()
		{
			InitializeComponent();

			pictureBoxAnimation.SizeMode = PictureBoxSizeMode.Zoom; 
			pictureBoxAnimation.Image = Properties.Resources.joystickani;

			// Initialer Status
			labelStatus.Text = Properties.Resources.Txt_Splash_Initializing; // "Initialisiere Anwendung..." (aus Resx)
			labelStatus.TextAlign = ContentAlignment.MiddleCenter;

			// Progressbar initialisieren
			progressBar.Minimum = 0;
			progressBar.Maximum = 100;
			progressBar.Value = 0;
			progressBar.Style = ProgressBarStyle.Marquee; // Für unbestimmten Fortschritt
			progressBar.MarqueeAnimationSpeed = 30; // Geschwindigkeit des Laufbalkens
		}

		// Methoden zum Aktualisieren von Status und Fortschritt
		public void UpdateStatus(string statusText)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() => this.labelStatus.Text = statusText));
			}
			else
			{
				this.labelStatus.Text = statusText;
			}
		}

		public void UpdateProgress(int percentage)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() =>
				{
					if (this.progressBar.Style != ProgressBarStyle.Blocks)
					{
						this.progressBar.Style = ProgressBarStyle.Blocks;
					}
					this.progressBar.Value = percentage;
				}));
			}
			else
			{
				if (this.progressBar.Style != ProgressBarStyle.Blocks)
				{
					this.progressBar.Style = ProgressBarStyle.Blocks;
				}
				this.progressBar.Value = percentage;
			}
		}

		public void SetMarqueeProgress()
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() =>
				{
					this.progressBar.Style = ProgressBarStyle.Marquee;
					this.progressBar.MarqueeAnimationSpeed = 30;
				}));
			}
			else
			{
				this.progressBar.Style = ProgressBarStyle.Marquee;
				this.progressBar.MarqueeAnimationSpeed = 30;
			}
		}
	}

}

