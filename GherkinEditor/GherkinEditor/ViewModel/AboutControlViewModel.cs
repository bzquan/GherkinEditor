// -----------------------------------------------------------------------
// <copyright file="AboutControlViewModel.cs" company="">
//
// The MIT License (MIT)
// 
// Copyright (c) 2014 Christoph Gattnar
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of
// the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>
// -----------------------------------------------------------------------

namespace Gherkin.ViewModel
{
	using System;
	using System.ComponentModel;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Media;

	public class AboutControlViewModel : NotifyPropertyChangedBase
    {
		private ImageSource _ApplicationLogo;
		private string _Title;
		private string _Description;
		private string _Version;
		private ImageSource _PublisherLogo;
		private string _Copyright;
		private string _ReleaseNotes;
		private string _HyperlinkText;
		private Uri _Hyperlink;
        private string _Publisher;
        private bool _isSemanticVersioning;

		public AboutControlViewModel()
		{
			Window = new Window();
			Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			Window.SizeToContent = SizeToContent.WidthAndHeight;
			Window.ResizeMode = ResizeMode.NoResize;
			Window.WindowStyle = WindowStyle.None;

			Window.ShowInTaskbar = false;
			Window.Title = "About";
			Window.Deactivated += Window_Deactivated;

			Assembly assembly = Assembly.GetEntryAssembly();
			Version = assembly.GetName().Version.ToString();
			Title = assembly.GetName().Name;
			
#if NET35 || NET40
			AssemblyCopyrightAttribute copyright = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
			AssemblyDescriptionAttribute description = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
			AssemblyCompanyAttribute company = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
#else
			AssemblyCopyrightAttribute copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
			AssemblyDescriptionAttribute description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
			AssemblyCompanyAttribute company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
#endif
            Copyright = copyright.Copyright;
			Description = description.Description;
			Publisher = company.Company;
		}

		/// <summary>
		/// Gets or sets the application logo.
		/// </summary>
		/// <value>The application logo.</value>
		public ImageSource ApplicationLogo
		{
			get
			{
				return _ApplicationLogo;
			}
			set
			{
				if(_ApplicationLogo != value)
				{
					_ApplicationLogo = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets the application title.
		/// </summary>
		/// <value>The application title.</value>
		public string Title
		{
			get
			{
				return _Title;
			}
			set
			{
				if(_Title != value)
				{
					_Title = value;
                    OnPropertyChanged();
                }
            }
		}

		/// <summary>
		/// Gets or sets the application info.
		/// </summary>
		/// <value>The application info.</value>
		public string Description
		{
			get
			{
				return _Description;
			}
			set
			{
				if(_Description != value)
				{
					_Description = value;
                    OnPropertyChanged();
                }
            }
		}

        /// <summary>
        /// Gets or sets if Semantic Versioning is used.
        /// </summary>
        /// <see cref="http://semver.org/"/>
        /// <value>The bool that indicats whether Semantic Versioning is used.</value>
        public bool IsSemanticVersioning
        {
            get
            {
                return _isSemanticVersioning;
            }
            set
            {
                _isSemanticVersioning = value;
                OnPropertyChanged();
            }
        }

		/// <summary>
		/// Gets or sets the application version.
		/// </summary>
		/// <value>The application version.</value>
		public string Version
		{
			get
			{
                if (IsSemanticVersioning)
                {
                    var tmp = _Version.Split('.');
                    var version = string.Format("{0}.{1}.{2}", tmp[0], tmp[1], tmp[2]);
                    return version;
                }

				return _Version;
			}
			set
			{
				if(_Version != value)
				{
					_Version = value;
                    OnPropertyChanged();
                }
            }
		}

		/// <summary>
		/// Gets or sets the publisher logo.
		/// </summary>
		/// <value>The publisher logo.</value>
		public ImageSource PublisherLogo
		{
			get
			{
				return _PublisherLogo;
			}
			set
			{
				if(_PublisherLogo != value)
				{
					_PublisherLogo = value;
                    OnPropertyChanged();
                }
            }
		}

		/// <summary>
		/// Gets or sets the publisher.
		/// </summary>
		/// <value>The publisher.</value>
		public string Publisher
		{
			get
			{
				return _Publisher;
			}
			set
			{
				if(_Publisher != value)
				{
					_Publisher = value;
                    OnPropertyChanged();
                }
            }
		}

		/// <summary>
		/// Gets or sets the copyright label.
		/// </summary>
		/// <value>The copyright label.</value>
		public string Copyright
		{
			get
			{
				return _Copyright;
			}
			set
			{
				if(_Copyright != value)
				{
					_Copyright = value;
                    OnPropertyChanged();
                }
            }
		}

		/// <summary>
		/// Gets or sets the hyperlink text.
		/// </summary>
		/// <value>The hyperlink text.</value>
		public string HyperlinkText
		{
			get
			{
				return _HyperlinkText;
			}
			set
			{
				try
				{
					Hyperlink = new Uri(value);
					_HyperlinkText = value;
                    OnPropertyChanged();
                }
                catch
				{
				}
			}
		}

		public Uri Hyperlink
		{
			get
			{
				return _Hyperlink;
			}
			set
			{
				if(_Hyperlink != value)
				{
					_Hyperlink = value;
                    OnPropertyChanged();
                }
            }
		}

        /// <summary>
        /// Release note in HTML
        /// </summary>
        public string ReleaseNote
        {
            get
            {
                return _ReleaseNotes;
            }
            set
            {
                if (_ReleaseNotes != value)
                {
                    _ReleaseNotes = value;
                    OnPropertyChanged();
                }
            }
        }

		public Window Window
		{
			get;
			set;
		}

		void Window_Deactivated(object sender, System.EventArgs e)
		{
			Window.Close();
		}
	}
}
