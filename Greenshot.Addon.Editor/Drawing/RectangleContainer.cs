//  Greenshot - a free and open source screenshot tool
//  Copyright (C) 2007-2017 Thomas Braun, Jens Klingen, Robin Krom
// 
//  For more information see: http://getgreenshot.org/
//  The Greenshot project is hosted on GitHub: https://github.com/greenshot
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 1 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

#region Usings

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using Greenshot.Addon.Editor.Helpers;
using Greenshot.Addon.Editor.Interfaces.Drawing;
using Greenshot.Addon.Extensions;
using Greenshot.Core.Extensions;

#endregion

namespace Greenshot.Addon.Editor.Drawing
{
	/// <summary>
	///     Represents a rectangular shape on the Surface
	/// </summary>
	[Serializable]
	public class RectangleContainer : DrawableContainer
	{
		protected Color _fillColor = Color.Transparent;

		protected Color _lineColor = Color.Red;
		protected int _lineThickness = 2;

		protected bool _shadow = true;

		public RectangleContainer(Surface parent) : base(parent)
		{
			Init();
		}

		[Field(FieldTypes.FILL_COLOR)]
		public Color FillColor
		{
			get { return _fillColor; }
			set
			{
				_fillColor = value;
				OnFieldPropertyChanged(FieldTypes.FILL_COLOR);
			}
		}

		[Field(FieldTypes.LINE_COLOR)]
		public Color LineColor
		{
			get { return _lineColor; }
			set
			{
				_lineColor = value;
				OnFieldPropertyChanged(FieldTypes.LINE_COLOR);
			}
		}

		[Field(FieldTypes.LINE_THICKNESS)]
		public int LineThickness
		{
			get { return _lineThickness; }
			set
			{
				_lineThickness = value;
				OnFieldPropertyChanged(FieldTypes.LINE_THICKNESS);
			}
		}

		[Field(FieldTypes.SHADOW)]
		public bool Shadow
		{
			get { return _shadow; }
			set
			{
				_shadow = value;
				OnFieldPropertyChanged(FieldTypes.SHADOW);
			}
		}

		public override bool ClickableAt(int x, int y)
		{
			Rectangle rect = new Rectangle(Left, Top, Width, Height).MakeGuiRectangle();
			return RectangleClickableAt(rect, _lineThickness, _fillColor, x, y);
		}


		public override void Draw(Graphics graphics, RenderMode rm)
		{
			Rectangle rect = new Rectangle(Left, Top, Width, Height).MakeGuiRectangle();

			DrawRectangle(rect, graphics, rm, _lineThickness, _lineColor, _fillColor, _shadow);
		}

		/// <summary>
		///     This method can also be used from other containers, if the right values are passed!
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="graphics"></param>
		/// <param name="rm"></param>
		/// <param name="lineThickness"></param>
		/// <param name="lineColor"></param>
		/// <param name="fillColor"></param>
		/// <param name="shadow"></param>
		public static void DrawRectangle(Rectangle rect, Graphics graphics, RenderMode rm, int lineThickness, Color lineColor, Color fillColor, bool shadow)
		{
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.None;

			bool lineVisible = (lineThickness > 0) && ColorHelper.IsVisible(lineColor);
			if (shadow && (lineVisible || ColorHelper.IsVisible(fillColor)))
			{
				//draw shadow first
				int basealpha = 100;
				int alpha = basealpha;
				int steps = 5;
				int currentStep = lineVisible ? 1 : 0;
				while (currentStep <= steps)
				{
					using (Pen shadowPen = new Pen(Color.FromArgb(alpha, 100, 100, 100)))
					{
						shadowPen.Width = lineVisible ? lineThickness : 1;
						Rectangle shadowRect = new Rectangle(rect.Left + currentStep, rect.Top + currentStep, rect.Width, rect.Height).MakeGuiRectangle();
						graphics.DrawRectangle(shadowPen, shadowRect);
						currentStep++;
						alpha = alpha - basealpha/steps;
					}
				}
			}


			if (ColorHelper.IsVisible(fillColor))
			{
				using (Brush brush = new SolidBrush(fillColor))
				{
					graphics.FillRectangle(brush, rect);
				}
			}

			graphics.SmoothingMode = SmoothingMode.HighSpeed;
			if (lineVisible)
			{
				using (Pen pen = new Pen(lineColor, lineThickness))
				{
					graphics.DrawRectangle(pen, rect);
				}
			}
		}

		private void Init()
		{
			CreateDefaultAdorners();
		}

		/// <summary>
		///     Do some logic to make sure all field are initiated correctly
		/// </summary>
		/// <param name="streamingContext">StreamingContext</param>
		protected override void OnDeserialized(StreamingContext streamingContext)
		{
			base.OnDeserialized(streamingContext);
			Init();
		}

		public static bool RectangleClickableAt(Rectangle rect, int lineThickness, Color fillColor, int x, int y)
		{
			// If we clicked inside the rectangle and it's visible we are clickable at.
			if (!Color.Transparent.Equals(fillColor))
			{
				if (rect.Contains(x, y))
				{
					return true;
				}
			}

			// check the rest of the lines
			if (lineThickness > 0)
			{
				using (Pen pen = new Pen(Color.White, lineThickness))
				{
					using (GraphicsPath path = new GraphicsPath())
					{
						path.AddRectangle(rect);
						return path.IsOutlineVisible(x, y, pen);
					}
				}
			}
			return false;
		}
	}
}