using System;
using System.Configuration;
using System.Web; 
using System.Net;
using System.Text;
using System.IO;


namespace ReverseProxy
{
	public class ReverseProxy: IHttpHandler
	{
        string proxyUrl {
            get {
                return ConfigurationSettings.AppSettings["ProxyUrl"];
            }
        }
	
		public void ProcessRequest(HttpContext context)
		{			
			// Create the web request to communicate with the back-end site
			string remoteUrl;
			remoteUrl = context.Request.Url.AbsoluteUri.Replace("http://"+context.Request.Url.Host+context.Request.ApplicationPath, proxyUrl);
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(remoteUrl);
			request.Method = context.Request.HttpMethod;
            request.Headers["Remote-User"] = HttpContext.Current.User.Identity.Name;
            foreach(String each in context.Request.Headers)
                if (!WebHeaderCollection.IsRestricted(each)) 
                    request.Headers.Add(each, context.Request.Headers.Get(each));           
            if (context.Request.HttpMethod == "POST")
            {
                Stream outputStream = request.GetRequestStream();
                CopyStream(context.Request.InputStream, outputStream);
                outputStream.Close();
            }

			HttpWebResponse response;
			try
			{
				response = (HttpWebResponse) request.GetResponse();
			}
			catch(System.Net.WebException we)
			{
			    response = (HttpWebResponse) we.Response;
			    context.Response.StatusCode = (int) response.StatusCode;
			    // TBD: handle case where we.Response is null
			}

            // Copy response from server back to client
            foreach(String each in response.Headers)
                context.Response.AddHeader(each, response.Headers.Get(each));           
			CopyStream(response.GetResponseStream(), context.Response.OutputStream);
			response.Close();
			context.Response.End();
		}
		
        static public void CopyStream(Stream input, Stream output) {
            Byte[] buffer = new byte[1024];
            int bytes = 0;
            while( (bytes = input.Read(buffer, 0, 1024) ) > 0 )
                output.Write(buffer, 0, bytes);
        }
 
		public bool IsReusable
		{
			get
			{
				return true;
			}
		}	
	}
}
