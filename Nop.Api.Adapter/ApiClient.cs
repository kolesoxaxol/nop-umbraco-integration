﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace Nop.Api.Adapter
{
    public class ApiClient
    {
        protected const string DefaultContentType = "application/json";

        public object Call(HttpMethods method, string path)
        {
            return Call(method, path, string.Empty);
        }

        public object Call(HttpMethods method, string path, object callParams)
        {
            string requestUriString = $"{AccessProvider.ServerUrl}/{path}";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);

            httpWebRequest.ContentType = DefaultContentType;

            httpWebRequest.Headers.Add("Authorization", $"Bearer {AccessProvider.Token}");

            httpWebRequest.Method = ((object)method).ToString();

            if (callParams != null)
            {
                if (method == HttpMethods.Get || method == HttpMethods.Delete)
                {
                    return $"{requestUriString}?{callParams}";
                }

                if (method == HttpMethods.Post || method == HttpMethods.Put)
                {
                    using (new MemoryStream())
                    {
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWriter.Write(callParams);
                            streamWriter.Close();
                        }
                    }
                }
            }

            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch(WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var provider = new AccessProvider();

                        provider.GetAccessToken();
                        
                        return Call(method, path, callParams);
                    }
                }
                throw new Exception($"Error call api {ex.Status}, request is {requestUriString}",ex);
            }

            string encodedData = string.Empty;

            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream);
                    encodedData = streamReader.ReadToEnd();
                    streamReader.Close();
                }
            }

            return encodedData;
        }

        public object Get(string path)
        {
            return Get(path, null);
        }

        public object Get(string path, NameValueCollection callParams)
        {
            return Call(HttpMethods.Get, path, callParams);
        }

        public object Post(string path, object data)
        {
            return Call(HttpMethods.Post, path, data);
        }

        public object Put(string path, object data)
        {
            return Call(HttpMethods.Put, path, data);
        }

        public object Delete(string path)
        {
            return Call(HttpMethods.Delete, path, null);
        }
    }
}