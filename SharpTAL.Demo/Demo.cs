﻿//
// Demo.cs
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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;

using SharpTAL.TemplateProgram;
using SharpTAL.TemplateCache;

namespace SharpTAL.Demo
{
	public static class DemoExtensions
	{
		public static string XmlToString(this XmlDocument xml)
		{
			var sb = new StringBuilder();
			var sw = new StringWriter(sb);
			xml.Save(sw);
			return sb.ToString();
		}

		public static string ToUpperExtension(this string s)
		{
			return s.ToUpper();
		}
	}

	public class Friend
	{
		public string Name;
		public int Age;
	}

	public class XmlDocumentList : List<XmlDocument>
	{
	}

	public class XmlDocumentDictRecursive : Dictionary<XmlDocument, XmlDocumentDictRecursive>
	{
	}

	class Demo
	{
		static void Main()
		{
			// Referenced Assemblies
			var refAssemblies = new List<Assembly> { typeof(Demo).Assembly };

			// Globals
			var globals = new Dictionary<string, object>
            {
                {
                    "friends", new List<Friend>
                    {
                        new Friend { Name="Samantha", Age=33 },
                        new Friend { Name="Kim", Age=35 },
                        new Friend { Name="Sandra", Age=22 },
                        new Friend { Name="Natalie", Age=20 }
                    }
                }
            };
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(Resources.Macros);
			globals.Add("xmlDocsList", new XmlDocumentList { xmlDoc });
			globals.Add("xmlDocsDictRecursive", new XmlDocumentDictRecursive
				{
					{ xmlDoc, new XmlDocumentDictRecursive { { xmlDoc, null } } }
				});
			globals.Add("xmlDocsDictListTuple", new Dictionary<XmlDocument, List<Tuple<string, string>>>
				{
					{ xmlDoc, new List<Tuple<string, string>> { new Tuple<string, string>("a", "b") } }
				});

			// Globals types
			var globalsTypes = new Dictionary<string, Type>();
			foreach (var kw in globals)
			{
				globalsTypes.Add(kw.Key, kw.Value.GetType());
			}

			try
			{
				var sw = new Stopwatch();

				// Basic test
				Console.WriteLine("Basic tests:");
				Console.WriteLine("=======================================");
				sw.Start();
				Console.WriteLine(new Template("Hello ${w}!").Render(new Dictionary<string, object> { { "w", "world" } }));
				sw.Stop();
				Console.WriteLine("{0} ms", sw.ElapsedMilliseconds);

				// Template program generator speed tests
				Console.WriteLine();
				Console.WriteLine("Template program generator speed tests:");
				Console.WriteLine("=======================================");
				sw.Reset();
				sw.Start();
				var programGenerator = new ProgramGenerator();
				for (int i = 0; i < 5; i++)
				{
					sw.Reset();
					sw.Start();
					var ti = new TemplateInfo
					{
						TemplateBody = Resources.Main,
						GlobalsTypes = globalsTypes,
						ReferencedAssemblies = refAssemblies
					};
					programGenerator.GenerateTemplateProgram(ref ti);
					sw.Stop();
					Console.WriteLine("{0}: {1} ms", i + 1, sw.ElapsedMilliseconds);
				}
				sw.Stop();

				// Template generation speed tests
				Console.WriteLine();
				Console.WriteLine("Template compilation speed tests:");
				Console.WriteLine("=================================");
				var template = new Template(Resources.Main, globalsTypes, refAssemblies);
				for (int i = 0; i < 5; i++)
				{
					sw.Reset();
					sw.Start();
					template.Compile();
					sw.Stop();
					Console.WriteLine("{0}: {1} ms", i + 1, sw.ElapsedMilliseconds);
				}

				// FS Template cache no. 1
				Console.WriteLine();
				Console.WriteLine("FS template cache no.1");
				Console.WriteLine("======================");
				string cacheFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Template Cache");
				if (!Directory.Exists(cacheFolder))
				{
					Directory.CreateDirectory(cacheFolder);
				}
				var cache1 = new FileSystemTemplateCache(cacheFolder, true, @"Demo_{key}.dll");
				template.TemplateCache = cache1;
				string result = "";
				for (int i = 0; i < 5; i++)
				{
					sw.Reset();
					sw.Start();
					result = template.Render(globals);
					sw.Stop();
					Console.WriteLine("{0}: {1} ms", i + 1, sw.ElapsedMilliseconds);
				}
				Console.WriteLine();

				// FS Template cache no. 2
				Console.WriteLine();
				Console.WriteLine("FS template cache no.2 (loading generated templates):");
				Console.WriteLine("=====================================================");
				var cache2 = new FileSystemTemplateCache(cacheFolder, false, @"Demo_{key}.dll");
				template.TemplateCache = cache2;
				for (int i = 0; i < 5; i++)
				{
					sw.Reset();
					sw.Start();
					template.Render(globals);
					sw.Stop();
					Console.WriteLine("{0}: {1} ms", i + 1, sw.ElapsedMilliseconds);
				}

				// Memory Template cache
				Console.WriteLine();
				Console.WriteLine("Memory template cache:");
				Console.WriteLine("======================");
				var cache3 = new MemoryTemplateCache();
				template.TemplateCache = cache3;
				for (int i = 0; i < 5; i++)
				{
					sw.Reset();
					sw.Start();
					template.Render(globals);
					sw.Stop();
					Console.WriteLine("{0}: {1} ms", i + 1, sw.ElapsedMilliseconds);
				}

				Console.WriteLine();
				Console.WriteLine("Render result:");
				Console.WriteLine("==============");
				Console.WriteLine(result);
			}
			catch (TemplateParseException ex)
			{
				Console.WriteLine("");
				Console.WriteLine("-------------------------------");
				Console.WriteLine(ex.Message);
				Console.WriteLine("-------------------------------");
			}
			catch (CompileSourceException ex)
			{
				Console.WriteLine("");
				Console.WriteLine("-------------------------------");
				Console.WriteLine(ex.Message);
				Console.WriteLine("-------------------------------");
			}
			catch (RenderTemplateException ex)
			{
				Console.WriteLine("");
				Console.WriteLine("-------------------------------");
				Console.WriteLine(ex.Message);
				Console.WriteLine("-------------------------------");
			}
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("-------------------------------");
				Console.WriteLine(ex.Message);
				Console.WriteLine("-------------------------------");
			}

			Console.WriteLine("");
			Console.WriteLine("Press any key ...");
			Console.ReadKey();
		}
	}
}
