// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;

namespace ICSharpCode.AvalonEdit.Search
{
	/// <summary>
	/// Holds default texts for buttons and labels in the SearchPanel. Override properties to add other languages.
	/// </summary>
	public class Localization
	{
        /// <summary>
        /// Default: 'Search term'
        /// </summary>
        public virtual string SearchTermText
        {
            get { return "Search term"; }
        }

        /// <summary>
        /// Default: 'Replacement term'
        /// </summary>
        public virtual string ReplacementTermText
        {
            get { return "Replacement term"; }
        }

        /// <summary>
        /// Default: 'Match case'
        /// </summary>
        public virtual string MatchCaseText {
			get { return "Match case"; }
		}
		
		/// <summary>
		/// Default: 'Match whole words'
		/// </summary>
		public virtual string MatchWholeWordsText {
			get { return "Match whole words"; }
		}

        /// <summary>
        /// Default: 'Use wild cars including ? or *'
        /// </summary>
        public virtual string UseWildCardsText
        {
            get { return "Use wild cards"; }
        }

        /// <summary>
        /// Default: 'Use regular expressions'
        /// </summary>
        public virtual string UseRegexText {
			get { return "Use regular expressions"; }
		}
		
		/// <summary>
		/// Default: 'Find next (F3)'
		/// </summary>
		public virtual string FindNextText {
			get { return "Find next (F3)"; }
		}
		
		/// <summary>
		/// Default: 'Find previous (Shift+F3)'
		/// </summary>
		public virtual string FindPreviousText {
			get { return "Find previous (Shift+F3)"; }
		}

        /// <summary>
        /// Default: 'Toggle to switch between find and replace modes'
        /// </summary>
        public virtual string ToggleFindReplace
        {
            get { return "Toggle to switch between find and replace modes"; }
        }
		/// <summary>
		/// Default: 'Replace next (ALT+R)'
		/// </summary>
		public virtual string ReplaceNextText {
			get { return "Replace next (ALT+R)"; }
		}

		/// <summary>
		/// Default: 'Replace all (ALT+A)'
		/// </summary>
		public virtual string ReplaceAllText {
			get { return "Replace all (ALT+A)"; }
		}

        /// <summary>
        /// Message in confirm message box for replace all
        /// </summary>
        public virtual string ReplaceAllConfirmMessage
        {
            get { return "Are you sure to Replace All occurences of \"{0}\" with \"{1}\"?"; }
        }

        /// <summary>
        /// Message box title for replace all
        /// </summary>
        public virtual string ReplaceAllConfirmTitle
        {
            get { return "Replace All"; }
        }

        /// <summary>
        /// Default: 'Error: '
        /// </summary>
        public virtual string ErrorText {
			get { return "Error: "; }
		}
		
		/// <summary>
		/// Default: 'No matches found!'
		/// </summary>
		public virtual string NoMatchesFoundText {
			get { return "No matches found!"; }
		}

        /// <summary>
        /// Default: 'Regex note'
        /// </summary>
        public virtual string RegexNoteTitle
        {
            get { return "Regex note"; }
        }

        /// <summary>
        /// Default: '<h5>Regex note not available</h5>'
        /// </summary>
        public virtual string RegexNote
        {
            get { return "<h5>Regex note not available</h5>"; }
        }
    }
}
