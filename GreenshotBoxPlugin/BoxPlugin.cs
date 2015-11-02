/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2015 Thomas Braun, Jens Klingen, Robin Krom, Francis Noel
 * 
 * For more information see: http://getgreenshot.org/
 * The Greenshot project is hosted on Sourceforge: http://sourceforge.net/projects/greenshot/
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using GreenshotPlugin.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using Dapplo.Addons;
using GreenshotPlugin.Interfaces;
using GreenshotPlugin.Interfaces.Plugin;
using GreenshotPlugin.Extensions;
using GreenshotPlugin.Interfaces.Destination;

namespace GreenshotBoxPlugin
{
	/// <summary>
	/// This is the Box base code
	/// </summary>
	[Plugin("Box", Configurable = true)]
	[StartupAction]
	public class BoxPlugin : IGreenshotPlugin, IStartupAction
	{
		private readonly ComponentResourceManager _resources = new ComponentResourceManager(typeof(BoxPlugin));
		private ToolStripMenuItem _itemPlugInConfig;

		[Import]
		public IGreenshotHost GreenshotHost
		{
			get;
			set;
		}

		[Import]
		public IBoxConfiguration BoxConfiguration
		{
			get;
			set;
		}

		[Import]
		public IBoxLanguage BoxLanguage
		{
			get;
			set;
		}

		[Export]
		public IDestination BoxDestination
		{
			get
			{
				var destination = new BoxDestination();
				var boxIcon = (Bitmap) _resources.GetObject("Box");
                destination.Icon = boxIcon.ToBitmapSource();
				return destination;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_itemPlugInConfig != null)
				{
					_itemPlugInConfig.Dispose();
					_itemPlugInConfig = null;
				}
			}
		}

		/// <summary>
		/// Initialize
		/// </summary>
		/// <param name="token"></param>
		public Task StartAsync(CancellationToken token = new CancellationToken())
		{
			_itemPlugInConfig = new ToolStripMenuItem
			{
				Image = (Image)_resources.GetObject("Box"),
				Text = BoxLanguage.Configure
			};
			_itemPlugInConfig.Click += (sender, eventArgs) => Configure();

			PluginUtils.AddToContextMenu(GreenshotHost, _itemPlugInConfig);
			BoxLanguage.PropertyChanged += (sender, args) =>
			{
				if (_itemPlugInConfig != null)
				{
					_itemPlugInConfig.Text = BoxLanguage.Configure;
				}
			};
			return Task.FromResult(true);
		}

		/// <summary>
		/// Implementation of the IPlugin.Configure
		/// </summary>
		public void Configure()
		{
			new SettingsForm(BoxConfiguration).ShowDialog();
		}
	}
}