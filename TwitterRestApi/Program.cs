using System;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

namespace TwitterRestApi
{
  class Program
  {
    static void Main(string[] args)
    {
      //you'll first need to provision a twitter client app for access: https://apps.twitter.com/
      //then plug in the following magic cookies from here: https://apps.twitter.com/app/{your_app_id}/keys
      var oauth = new OAuth.Manager
      {
        ["consumer_key"] = "",
        ["consumer_secret"] = "",
        ["token"] = "",
        ["token_secret"] = ""
      };

      //here's the api docs: https://dev.twitter.com/rest/public
      //e.g. https://dev.twitter.com/rest/reference/get/followers/ids
      //they all have a nice OAuth "signature generator" at the bottom that will create a valid curl command line
      //i didn't have any success with the curl approach but "PostMan" plugin for Chrome worked right away

      //following example loops over all your followers and removes them

      var client = new WebClient();

      var yourScreenName = "";

      var url = $"https://api.twitter.com/1.1/followers/ids.json?screen_name={yourScreenName}";
      client.Headers["Authorization"] = oauth.GenerateAuthzHeader(url, "GET");
      var result = client.DownloadString(url);

      var jobj = JObject.Parse(result);
      var value = jobj.GetValue("ids");
      var ids = value.ToObject<string[]>();

      foreach (var id in ids)
      {
        //removing a follower translates to blocking them and then unblocking
        //block
        url = $"https://api.twitter.com/1.1/blocks/create.json?user_id={id}";
        client.Headers["Authorization"] = oauth.GenerateAuthzHeader(url, "POST"); ;
        result = client.UploadString(url, "");

        //then unblock
        url = $"https://api.twitter.com/1.1/blocks/destroy.json?user_id={id}";
        client.Headers["Authorization"] = oauth.GenerateAuthzHeader(url, "POST"); ;
        result = client.UploadString(url, "");
      }

    }
  }

  class WebClientEx : WebClient
  {
    protected override WebRequest GetWebRequest(Uri address)
    {
      var request = base.GetWebRequest(address);

      // Perform any customizations on the request.
      // This version of WebClient always preauthenticates.
      Debug.Assert(request != null, "request != null");

      request.PreAuthenticate = true;
      //request.AllowWriteStreamBuffering = true;

      return request;
    }
  }
}
