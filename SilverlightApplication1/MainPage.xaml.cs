using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Browser;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightApplication1
{
	public partial class MainPage : UserControl
	{
		public MainPage()
		{
			InitializeComponent();

			var webRequest = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(new Uri("http://localhost:8080/"));
			webRequest.AllowReadStreamBuffering = false;
			webRequest.Method = "GET";
			webRequest.Headers["test"] = "true";

			Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse, null)
				.ContinueWith(task =>
				{
					var responseStream = task.Result.GetResponseStream();
					ReadAsync(responseStream);
				});
		}
		byte[] buffer = new byte[128];
		private int posInBuffer;
		private void ReadAsync(Stream responseStream)
		{
			Task.Factory.FromAsync<int>((callback, o) => responseStream.BeginRead(buffer, posInBuffer, buffer.Length- posInBuffer, callback, o),
			                       responseStream.EndRead, null)
								   .ContinueWith(task =>
								   {
									   var read = task.Result;
									   if (read == 0)// will for a reopening of the connection
										   throw new EndOfStreamException();
									   // find \r\n in newly read range

									   var startPos = 0;
									   byte prev = 0;
									   bool foundLines = false;
									   for (int i = posInBuffer; i < posInBuffer + read; i++)
									   {
										   if (prev == '\r' && buffer[i] == '\n')
										   {
											   foundLines = true;
											   // yeah, we found a line, let us give it to the users
											   var data = Encoding.UTF8.GetString(buffer, startPos, i - 1 - startPos);
											   startPos = i + 1;
										   	Dispatcher.BeginInvoke(() =>
										   	{
										   		ServerResults.Text += data + Environment.NewLine;
										   	});
										   }
										   prev = buffer[i];
									   }
									   posInBuffer += read;
									   if (startPos >= posInBuffer) // read to end
									   {
										   posInBuffer = 0;
										   return;
									   }
									   if (foundLines == false)
										   return;

									   // move remaining to the start of buffer, then reset
									   Array.Copy(buffer, startPos, buffer, 0, posInBuffer - startPos);
									   posInBuffer -= startPos;
								   })
								   .ContinueWith(task =>
								   {
									   if (task.IsFaulted)
									   	return;
									   ReadAsync(responseStream);
								   });
		}
	}
}
