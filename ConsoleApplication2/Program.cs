using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ConsoleApplication2
{
	class Program
	{
		static void Main(string[] args)
		{
			var listener = new HttpListener
			{
				Prefixes = {"http://+:8080/"}
			};
			listener.Start();
			while (true)
			{
				var ctx = listener.GetContext();
				Console.WriteLine(ctx.Request.RawUrl);
				if (string.Equals(ctx.Request.RawUrl, "/clientaccesspolicy.xml", StringComparison.InvariantCultureIgnoreCase))
				{
					using (var writer = new StreamWriter(ctx.Response.OutputStream))
					{
						writer.Write(@"<?xml version='1.0' encoding='utf-8'?>
<access-policy>
 <cross-domain-access>
   <policy>
	 <allow-from http-methods='*' http-request-headers='*'>
	   <domain uri='*' />
	 </allow-from>
	 <grant-to>
	   <resource include-subpaths='true' path='/' />
	 </grant-to>
   </policy>
 </cross-domain-access>
</access-policy>");
						writer.Flush();
					}
					ctx.Response.Close();
					continue;
				}


				using (var writer = new StreamWriter(ctx.Response.OutputStream))
				{
					/*
					 Here it does NOT works
					 
					writer.WriteLine("first\r\nsecond");
					writer.Flush();
					*/

					/*
					 * same output as before, still doesn't work
					writer.WriteLine("first");
					writer.WriteLine("second");
					writer.Flush();
					 */

					/*
					 * Doesn't work
					writer.WriteLine("first");
					writer.Flush();
					writer.WriteLine("second");
					writer.Flush();
					*/

					// sometimes works
					//writer.WriteLine("first");
					//writer.Flush();
					//Thread.Sleep(10);
					//writer.WriteLine("second");
					//writer.Flush();

					//works
					//writer.WriteLine("first");
					//writer.Flush();
					//Thread.Sleep(50);
					//writer.WriteLine("second");
					//writer.Flush();
					Console.WriteLine("written");
					Console.ReadLine();
				}
			}
		}
	}
}
