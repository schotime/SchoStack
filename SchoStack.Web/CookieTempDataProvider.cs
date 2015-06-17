using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Mvc;

namespace SchoStack.Web
{
    public class CookieTempDataProvider : ITempDataProvider
    {
        internal const string TempDataCookieKey = "__TempData";
        readonly HttpContextBase _httpContext;

        public CookieTempDataProvider(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }
            _httpContext = httpContext;
        }

        public HttpContextBase HttpContext
        {
            get
            {
                return _httpContext;
            }
        }

        protected virtual IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
        {
            HttpCookie cookie = _httpContext.Request.Cookies[TempDataCookieKey];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                IDictionary<string, object> deserializedTempData = DeserializeTempData(cookie.Value);

                cookie.Expires = DateTime.Now.AddDays(-30);
                cookie.Value = string.Empty;
                cookie.Path = controllerContext.HttpContext.Request.ApplicationPath;
                cookie.Secure = controllerContext.HttpContext.Request.IsSecureConnection;
                cookie.HttpOnly = true;

                if (_httpContext.Response != null && _httpContext.Response.Cookies != null)
                {
                    _httpContext.Response.Cookies.Add(cookie);
                }

                return deserializedTempData;
            }

            return new Dictionary<string, object>();
        }

        protected virtual void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
        {
            if (!values.Any())
            {
                var cookied = new HttpCookie(TempDataCookieKey)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(-30),
                    Path = controllerContext.HttpContext.Request.ApplicationPath,
                    Secure = controllerContext.HttpContext.Request.IsSecureConnection
                };
                _httpContext.Response.Cookies.Add(cookied);
                return;
            }
                
            var cookieValue = SerializeToBase64EncodedString(values);

            var cookie = new HttpCookie(TempDataCookieKey)
            {
                HttpOnly = true,
                Value = cookieValue,
                Path = controllerContext.HttpContext.Request.ApplicationPath,
                Secure = controllerContext.HttpContext.Request.IsSecureConnection
            };

            _httpContext.Response.Cookies.Add(cookie);
        }

        public static IDictionary<string, object> DeserializeTempData(string base64EncodedSerializedTempData)
        {
            var bytes = Convert.FromBase64String(base64EncodedSerializedTempData);
            var memStream = new MemoryStream(bytes);
            var binFormatter = new BinaryFormatter();
            return binFormatter.Deserialize(memStream, null) as IDictionary<string, object>;
        }

        public static string SerializeToBase64EncodedString(IDictionary<string, object> values)
        {
            var memStream = new MemoryStream();
            memStream.Seek(0, SeekOrigin.Begin);
            var binFormatter = new BinaryFormatter();
            binFormatter.Serialize(memStream, values);
            memStream.Seek(0, SeekOrigin.Begin);
            var bytes = memStream.ToArray();
            return Convert.ToBase64String(bytes);
        }

        IDictionary<string, object> ITempDataProvider.LoadTempData(ControllerContext controllerContext)
        {
            return LoadTempData(controllerContext);
        }

        void ITempDataProvider.SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
        {
            SaveTempData(controllerContext, values);
        }
    }
}