/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2015 Thomas Braun, Jens Klingen, Robin Krom
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

using Dapplo.Config.Ini;
using Greenshot.Plugin;
using GreenshotPlugin.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapplo.Config.Language;
using Dapplo.Addons;

namespace GreenshotPhotobucketPlugin
{
	/// <summary>
	/// This is the GreenshotPhotobucketPlugin base code
	/// </summary>
	[Plugin(Configurable = true)]
	[StartupAction]
    public class PhotobucketPlugin : IConfigurablePlugin, IStartupAction, IShutdownAction
	{
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof (PhotobucketPlugin));
		private static IPhotobucketConfiguration _config;
		private static IPhotobucketLanguage _language;

		private ComponentResourceManager _resources;
		private ToolStripMenuItem _itemPlugInConfig;

		[Import]
		public IGreenshotHost GreenshotHost
		{
			get;
			set;
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

		public IEnumerable<IDestination> Destinations()
		{
			yield return new PhotobucketDestination();
		}

		public IEnumerable<IProcessor> Processors()
		{
			yield break;
		}

		/// <summary>
		/// Initialize
		/// </summary>
		/// <param name="token"></param>
		public async Task StartAsync(CancellationToken token = new CancellationToken())
		{
			// Register / get the photobucket configuration
			_config = await IniConfig.Current.RegisterAndGetAsync<IPhotobucketConfiguration>(token);
			_language = await LanguageLoader.Current.RegisterAndGetAsync<IPhotobucketLanguage>(token);
			_resources = new ComponentResourceManager(typeof (PhotobucketPlugin));


			_itemPlugInConfig = new ToolStripMenuItem(_language.Configure)
			{
				Tag = GreenshotHost
			};
			_itemPlugInConfig.Click += (sender, eventArgs) => Configure();
			_itemPlugInConfig.Image = (Image) _resources.GetObject("Photobucket");

			PluginUtils.AddToContextMenu(GreenshotHost, _itemPlugInConfig);
			_language.PropertyChanged += OnLanguageChanged;
		}

		public void OnLanguageChanged(object sender, EventArgs e)
		{
			if (_itemPlugInConfig != null)
			{
				_itemPlugInConfig.Text = _language.Configure;
			}
		}

		/// <summary>
		/// Implementation of the IPlugin.Configure
		/// </summary>
		public void Configure()
		{
			var settingsForm = new SettingsForm(_config);
			settingsForm.ShowDialog();
		}


		public Task ShutdownAsync(CancellationToken token = new CancellationToken())
		{
			_language.PropertyChanged -= OnLanguageChanged;
			return Task.FromResult(true);
		}
	}
}