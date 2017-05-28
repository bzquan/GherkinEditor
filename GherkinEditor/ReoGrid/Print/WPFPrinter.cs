﻿/*****************************************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * http://reogrid.net/
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Author: Jing <lujing at unvell.com>
 *
 * Copyright (c) 2012-2016 Jing <lujing at unvell.com>
 * Copyright (c) 2012-2016 unvell.com, all rights reserved.
 * 
 ****************************************************************************/

#if PRINT

#if WPF

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Xps;
using System.Windows.Xps.Packaging;

using unvell.ReoGrid.Print;

namespace unvell.ReoGrid.Print
{
	partial class PrintSession
	{
		internal void Init() { }

		public void Dispose() { }

		/// <summary>
		/// Start output document to printer.
		/// </summary>
		public void Print()
		{
			throw new NotImplementedException("WPF Print is not implemented yet. Try use Windows Form version to print document as XPS file.");
		}
	}
}

namespace unvell.ReoGrid
{
	partial class Worksheet
	{
	}
}

#endif // WPF

#endif // PRINT