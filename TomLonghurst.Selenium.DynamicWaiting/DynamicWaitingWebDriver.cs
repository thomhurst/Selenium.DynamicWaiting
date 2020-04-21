using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;

namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class DynamicWaitingWebDriver : EventFiringWebDriver
    {
        private readonly IWebDriver _webDriver;
        private readonly IEnumerable<DynamicWaitingRule> _dynamicWaitingRules;

        public DynamicWaitingWebDriver(IWebDriver parentDriver, IEnumerable<DynamicWaitingRule> dynamicWaitingRules) :
            base(parentDriver)
        {
            _dynamicWaitingRules = dynamicWaitingRules ?? Enumerable.Empty<DynamicWaitingRule>();
            _webDriver = parentDriver ?? throw new ArgumentNullException(nameof(_webDriver));

            SetupEvents();
        }
        
        public static bool IsDynamicWaitingWebDriver(IWebDriver driver, out DynamicWaitingWebDriver dynamicWaitingWebDriver)
        {
            if (driver is DynamicWaitingWebDriver dynamicDriver)
            {
                dynamicWaitingWebDriver = dynamicDriver;
                return true;
            }

            if (driver is IWrapsDriver wrapsDriver && IsDynamicWaitingWebDriver(wrapsDriver.WrappedDriver, out dynamicWaitingWebDriver))
            {
                return true;
            }

            dynamicWaitingWebDriver = null;
            return false;
        }

        private void SetupEvents()
        {
            Navigated += (sender, args) => ExecuteDynamicWait();
            NavigatedBack += (sender, args) => ExecuteDynamicWait();
            NavigatedForward += (sender, args) => ExecuteDynamicWait();
            ElementClicked += (sender, args) => ExecuteDynamicWait();
            ElementValueChanged += (sender, args) => ExecuteDynamicWait();
            FindingElement += (sender, args) => ExecuteDynamicWait();
            ScriptExecuted += (sender, args) => ExecuteDynamicWait();
        }

        public new ITargetLocator SwitchTo()
        {
            return new DynamicWaitingSwitchTo(this);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void ExecuteDynamicWait()
        {
            try
            {
                new WebDriverWait(_webDriver, TimeSpan.FromSeconds(15))
                    .Until(driver =>
                        ExecuteJavascriptPageFinishedLoadingCheck(driver,
                            JavascriptStatements.DocumentReadyStateComplete));
            }
            catch (Exception)
            {
                // This isn't one of the DyanmicWaitingRules passed to us - So don't throw exceptions here.
                // This is just to try and make sure the base page has loaded.
            }

            try
            {
                var currentHost = CurrentHost;

                foreach (var dynamicWaitingRule in _dynamicWaitingRules.Where(dynamicWaitingRule =>
                    currentHost.Contains(dynamicWaitingRule.Host)))
                {
                    new WebDriverWait(_webDriver, dynamicWaitingRule.Timeout)
                        .Until(driver =>
                            ExecuteJavascriptPageFinishedLoadingCheck(driver, dynamicWaitingRule.Javascript));
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
            }
        }

        private static bool ExecuteJavascriptPageFinishedLoadingCheck(IWebDriver driver, string javascript)
        {
            if (!(driver is IJavaScriptExecutor javaScriptExecutor))
            {
                return true;
            }

            var executeScriptResult = javaScriptExecutor.ExecuteScript(javascript);

            if (executeScriptResult is bool hasPageFinishedLoading)
            {
                return hasPageFinishedLoading;
            }

            if (bool.TryParse(executeScriptResult.ToString(), out hasPageFinishedLoading))
            {
                return hasPageFinishedLoading;
            }

            return true;
        }

        private string CurrentHost
        {
            get
            {
                if (!(_webDriver is IJavaScriptExecutor javaScriptExecutor))
                {
                    return GetHost(_webDriver.Url);
                }
                
                return GetHost((string) javaScriptExecutor.ExecuteScript("return document.URL"));
            }
        }

        private static string GetHost(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                return uri.Host;
            }

            return string.Empty;
        }
    }
}