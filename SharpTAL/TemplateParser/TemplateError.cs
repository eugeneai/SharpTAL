﻿//
// TemplateError.cs
//
// Author:
//   Roman Lacko (backup.rlacko@gmail.com)
//
// Copyright (c) 2010 - 2014 Roman Lacko
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace SharpTAL.TemplateParser
{
	public class TemplateError : Exception
	{
		private readonly string _msg;
		private readonly Token _token;
		private readonly string _filename;

		public TemplateError(string msg, Token token)
		{
			_msg = msg;
			_token = token;
			_filename = token.Filename;
		}

		public override string Message
		{
			get
			{
				string text = string.Format("{0}\n\n", _msg);
				text += string.Format("   - String:   \"{0}\"", _token);

				if (_filename != null)
				{
					text += "\n";
					text += string.Format("   - Filename: {0}", _filename);
				}

				Location loc = _token.Location;
				text += "\n";
				text += string.Format("   - Location: ({0}:{1})", loc.Line, loc.Position);

				return text;
			}
		}

		public int Offset
		{
			get
			{
				return _token.Position;
			}
		}
	}
}
