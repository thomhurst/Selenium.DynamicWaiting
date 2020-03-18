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

        public DynamicWaitingWebDriver(IWebDriver parentDriver, IEnumerable<DynamicWaitingRule> dynamicWaitingRules) : base(parentDriver)
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
            new WebDriverWait(_webDriver, TimeSpan.FromSeconds(15)).Until(driver =>
                (bool) ((IJavaScriptExecutor) driver).ExecuteScript("return document.readyState == \"complete\""));
            
            foreach (var dynamicWaitingRule in _dynamicWaitingRules.Where(dynamicWaitingRule => CurrentHost.Contains(dynamicWaitingRule.Host)))
            {
                new WebDriverWait(_webDriver, dynamicWaitingRule.Timeout).Until(driver =>
                    (bool) ((IJavaScriptExecutor) driver).ExecuteScript(dynamicWaitingRule.Javascript));
            }
        }

        private string CurrentHost => new Uri(Url).Host;
    }
}