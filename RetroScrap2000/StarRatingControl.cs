using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroScrap2000
{
	public partial class StarRatingControl : UserControl
	{
		private int _starCount = 5;
		private double _rating = 0.0;        // 0..StarCount (Schrittweite 0.5)
		private double? _hoverRating = null; // temporär bei MouseOver

		[Category("Appearance")]
		public int StarCount
		{
			get => _starCount;
			set { _starCount = Math.Max(1, value); Invalidate(); }
		}

		[Category("Behavior")]
		public bool AllowHalfStars { get; set; } = true;

		[Category("Appearance")]
		public int StarSpacing { get; set; } = 4;

		[Category("Appearance")]
		public Color FilledColor { get; set; } = Color.Gold;

		[Category("Appearance")]
		public Color EmptyColor { get; set; } = Color.LightGray;

		[Category("Appearance")]
		public Color OutlineColor { get; set; } = Color.Black;

		/// <summary>Aktuelles Rating (0..StarCount, i. d. R. in 0.5-Schritten)</summary>
		[Category("Behavior")]
		public double Rating
		{
			get => _rating;
			set
			{
				var clamped = Math.Max(0, Math.Min(StarCount, value));
				if (Math.Abs(_rating - clamped) > double.Epsilon)
				{
					_rating = clamped;
					Invalidate();
					RatingChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		[Category("Action")]
		public event EventHandler? RatingChanged;

		public StarRatingControl()
		{
			DoubleBuffered = true;
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
							 ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			Height = 28;
			Width = 32 * _starCount + StarSpacing * (_starCount - 1);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			double effective = _hoverRating ?? _rating;

			// verfügbare Breite je Stern
			int totalSpacing = StarSpacing * (StarCount - 1);
			int starWidth = (Width - totalSpacing) / StarCount;
			int size = Math.Min(starWidth, Height);

			for (int i = 0; i < StarCount; i++)
			{
				int x = i * (size + StarSpacing);
				int y = (Height - size) / 2;

				using var starPath = CreateStarPath(new Rectangle(x, y, size, size), 5, 0.5f);

				// Wie weit ist dieser Stern gefüllt? (0..1)
				double remaining = effective - i;
				float fill = (float)Math.Max(0, Math.Min(1, remaining));
				if (AllowHalfStars) fill = (float)(Math.Round(fill * 2, MidpointRounding.AwayFromZero) / 2.0);
				else fill = fill >= 1 ? 1f : 0f;

				// Hintergrund (leer)
				using (var brEmpty = new SolidBrush(EmptyColor))
					e.Graphics.FillPath(brEmpty, starPath);

				// Teilfüllung (links-bündig clippen)
				if (fill > 0f)
				{
					var bounds = starPath.GetBounds();
					var clipRect = new RectangleF(bounds.Left, bounds.Top, bounds.Width * fill, bounds.Height);
					var prevClip = e.Graphics.Clip;
					e.Graphics.SetClip(clipRect, CombineMode.Replace);
					using (var brFilled = new SolidBrush(FilledColor))
						e.Graphics.FillPath(brFilled, starPath);
					e.Graphics.SetClip(prevClip, CombineMode.Replace);
				}

				// Kontur
				using var pen = new Pen(OutlineColor, 1f);
				e.Graphics.DrawPath(pen, starPath);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			var newHover = CalcRatingFromPoint(e.Location);
			if (_hoverRating == null || Math.Abs(_hoverRating.Value - newHover) > double.Epsilon)
			{
				_hoverRating = newHover;
				Invalidate();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			_hoverRating = null;
			Invalidate();
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			Rating = CalcRatingFromPoint(e.Location);
		}

		private double CalcRatingFromPoint(Point p)
		{
			int totalSpacing = StarSpacing * (StarCount - 1);
			int starWidth = (Width - totalSpacing) / StarCount;
			int size = Math.Min(starWidth, Height);

			for (int i = 0; i < StarCount; i++)
			{
				int x = i * (size + StarSpacing);
				var rect = new Rectangle(x, 0, size, Height);
				if (rect.Contains(p))
				{
					double local = (p.X - x) / (double)size; // 0..1
					if (AllowHalfStars)
						local = Math.Ceiling(local * 2) / 2.0;  // auf 0.5 runden (oben)
					else
						local = local >= 1 ? 1 : (local > 0 ? 1 : 0);

					return Math.Max(0, Math.Min(StarCount, i + local));
				}
			}

			// links außerhalb → 0, rechts außerhalb → Max
			return p.X < 0 ? 0 : StarCount;
		}

		/// <summary>Erzeugt einen Stern (Polygon) mit Außen-/Innenradius-Verhältnis.</summary>
		private static GraphicsPath CreateStarPath(Rectangle bounds, int points, float innerRatio)
		{
			var path = new GraphicsPath();
			float cx = bounds.Left + bounds.Width / 2f;
			float cy = bounds.Top + bounds.Height / 2f;
			float outerR = Math.Min(bounds.Width, bounds.Height) / 2f;
			float innerR = outerR * innerRatio;

			var pts = new PointF[points * 2];
			double angle = -Math.PI / 2; // Start nach oben
			double step = Math.PI / points;

			for (int i = 0; i < pts.Length; i++)
			{
				float r = (i % 2 == 0) ? outerR : innerR;
				pts[i] = new PointF(
						cx + (float)(Math.Cos(angle) * r),
						cy + (float)(Math.Sin(angle) * r));
				angle += step;
			}

			path.AddPolygon(pts);
			path.CloseFigure();
			return path;
		}
	}
}
