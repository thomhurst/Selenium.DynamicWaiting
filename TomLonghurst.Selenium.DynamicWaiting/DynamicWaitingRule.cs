using System;

namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class DynamicWaitingRule
    {
        public string Javascript { get; }
        public TimeSpan Timeout { get; }

        public DynamicWaitingRule(string javascriptThatReturnsTrueIfPageLoaded, TimeSpan timeout)
        {
            ValidateArguments(javascriptThatReturnsTrueIfPageLoaded, timeout);
            
            Javascript = javascriptThatReturnsTrueIfPageLoaded;
            Timeout = timeout;
        }

        public static DynamicWaitingRule JQuery(TimeSpan timeout) =>
            new DynamicWaitingRule(JavascriptStatements.JQuery, timeout);

        private static void ValidateArguments(string javascript, TimeSpan timeout)
        {
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