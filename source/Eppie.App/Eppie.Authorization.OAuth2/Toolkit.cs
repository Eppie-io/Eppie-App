using System;
using System.Collections.Specialized;
using System.Web;

namespace Tuvi.OAuth2
{
    public static class Toolkit
    {
        public static NameValueCollection ParseQueryString(string uriString)
        {
            try
            {
                Uri uri = new UriBuilder(uriString).Uri;
                return ParseQueryString(uri);
            }
            catch (UriFormatException)
            {
                return new NameValueCollection();
            }
        }

        public static NameValueCollection ParseQueryString(Uri uri)
        {
            try
            {
                return HttpUtility.ParseQueryString(uri?.Query);
            }
            catch (ArgumentNullException)
            {
                return new NameValueCollection();
            }
        }
    }
}
