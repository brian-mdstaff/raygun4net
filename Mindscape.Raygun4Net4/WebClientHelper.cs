using System;
using System.Net;

namespace Mindscape.Raygun4Net
{
  public static class WebClientHelper
  {
    [ThreadStatic] private static WebClient _client;

    private static WebClient Client => _client ?? (_client = new WebClient());
    private static readonly Uri ProxyUri;

    internal static IWebProxy WebProxy { get; set; }

    static WebClientHelper()
    {
      if (WebRequest.DefaultWebProxy != null)
      {
        var uri = new Uri(RaygunSettings.Settings.ApiEndpoint.ToString());
        ProxyUri = WebRequest.DefaultWebProxy.GetProxy(uri);
      }
    }

    public static void Send(string message, string apiKey, ICredentials proxyCredentials)
    {
      if (string.IsNullOrEmpty(apiKey))
      {
        System.Diagnostics.Trace.WriteLine("ApiKey has not been provided, the Raygun message will not be sent");
        return;
      }

      Client.Headers.Clear();
      Client.Headers.Add("X-ApiKey", apiKey);
      Client.Headers.Add("content-type", "application/json; charset=utf-8");
      Client.Encoding = System.Text.Encoding.UTF8;

      if (WebProxy != null)
      {
        Client.Proxy = WebProxy;
      }
      else if (WebRequest.DefaultWebProxy != null)
      {
        if (ProxyUri != null && ProxyUri.AbsoluteUri != RaygunSettings.Settings.ApiEndpoint.ToString())
        {
          Client.Proxy = new WebProxy(ProxyUri, false);

          if (proxyCredentials == null)
          {
            Client.UseDefaultCredentials = true;
            Client.Proxy.Credentials = CredentialCache.DefaultCredentials;
          }
          else
          {
            Client.UseDefaultCredentials = false;
            Client.Proxy.Credentials = proxyCredentials;
          }
        }
      }

      Client.UploadString(RaygunSettings.Settings.ApiEndpoint, message);
    }
  }
}