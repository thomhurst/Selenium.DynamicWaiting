using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;
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
            _webDriver = parentDriver;

            SetupEvents();
        }

        private void SetupEvents()
        {
            Navigated += (sender, args) => ExecuteDynamicWait();
            NavigatedBack += (sender, args) => ExecuteDynamicWait();
            NavigatedForward += (sender, args) => ExecuteDynamicWait();
            ElementClicked += (sender, args) => ExecuteDynamicWait();
            ElementValueChanged += (sender, args) => ExecuteDynamicWait();
            FindingElement += (sender, args) => ExecuteDynamicWait();
        }

        private void ExecuteDynamicWait()
        {
            try
            {
                new WebDriverWait(_webDriver, TimeSpan.FromSeconds(15))
                    .Until(driver =>
                        ExecuteJavascriptPageFinishedLoadingCheck(driver,
                            "return document.readyState == \"complete\""));
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
                Console.WriteLine(e);
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

        private string CurrentHost => new Uri(_webDriver.Url).Host;
    }
}