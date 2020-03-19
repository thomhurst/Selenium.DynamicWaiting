using System;

namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class DynamicWaitingRule
    {
        public string Host { get; }
        public string Javascript { get; }
        public TimeSpan Timeout { get; }

        public DynamicWaitingRule(string host, string javascriptThatReturnsTrueIfPageLoaded, TimeSpan timeout)
        {
            ValidateArguments(host, javascriptThatReturnsTrueIfPageLoaded, timeout);
            
            Host = host;
            Javascript = javascriptThatReturnsTrueIfPageLoaded;
            Timeout = timeout;
        }

        public static DynamicWaitingRule JQuery(string host, TimeSpan timeout) =>
            new DynamicWaitingRule(host, JavascriptStatements.JQuery, timeout);

        private static void ValidateArguments(string host, string javascript, TimeSpan timeout)
        {
            if(string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }
            
            if(string.IsNullOrEmpty(javascript))
            {
                throw new ArgumentNullException(nameof(javascript));
            }

            if (timeout == default)
            {
                throw new ArgumentNullException(nameof(timeout));
            }
        }
    }
}