﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class DynamicWaitingWebDriver : EventFiringWebDriver
    {
        public bool SkipNextWait { get; set; }
        internal DynamicWaitingSettings DynamicWaitingSettings { get; }
        internal bool IsDefaultContent { get; set; } = true;
        internal string OriginalWindowHandle { get; }
        private readonly IWebDriver _webDriver;
        private readonly IEnumerable<DynamicWaitingRule> _dynamicWaitingRules;
        private bool _isDisposing;


        public DynamicWaitingWebDriver(IWebDriver parentDriver, IEnumerable<DynamicWaitingRule> dynamicWaitingRules) : this(parentDriver, dynamicWaitingRules, DynamicWaitingSettings.Default)
        {
        }

        public DynamicWaitingWebDriver(IWebDriver parentDriver, IEnumerable<DynamicWaitingRule> dynamicWaitingRules, DynamicWaitingSettings dynamicWaitingSettings) :
            base(parentDriver)
        {
            _dynamicWaitingRules = dynamicWaitingRules ?? Enumerable.Empty<DynamicWaitingRule>();
            _webDriver = parentDriver ?? throw new ArgumentNullException(nameof(_webDriver));
            DynamicWaitingSettings = dynamicWaitingSettings ?? new DynamicWaitingSettings();

            OriginalWindowHandle = _webDriver.CurrentWindowHandle;
            
            SetupEvents();
        }

        public new string Url
        {
            get
            {
                if(CurrentWindowHasBeenClosed())
                {
                    SwitchToMainWindow();
                }
                
                return base.Url;
            }
            set
            {
                if (DynamicWaitingSettings.StopNavigatingToUrlActions?.Any(urlAction => urlAction.Action(value)) == true)
                {
                    return;
                }

                base.Url = value;
            }
        }

        public static bool TryGetDynamicWaitingWebDriver(IWebDriver driver, out DynamicWaitingWebDriver dynamicWaitingWebDriver)
        {
            switch (driver)
            {
                case DynamicWaitingWebDriver dynamicDriver:
                    dynamicWaitingWebDriver = dynamicDriver;
                    return true;
                case IWrapsDriver wrapsDriver when TryGetDynamicWaitingWebDriver(wrapsDriver.WrappedDriver, out dynamicWaitingWebDriver):
                    return true;
                default:
                    dynamicWaitingWebDriver = null;
                    return false;
            }
        }

        private void SetupEvents()
        {
            FindingElement += (sender, args) => ExecuteDynamicWait();
            
            if (DynamicWaitingSettings.WaitForPageLoadCompletion)
            {
                Navigated += (sender, args) => ExecuteDynamicWait();
                NavigatedBack += (sender, args) => ExecuteDynamicWait();
                NavigatedForward += (sender, args) => ExecuteDynamicWait();
            }

            ElementClicked += (sender, args) => ExecuteDynamicWait();
            
            ElementValueChanged += (sender, args) => ExecuteDynamicWait();

            if (DynamicWaitingSettings.WaitAfterScriptExecution)
            {
                ScriptExecuted += (sender, args) => ExecuteDynamicWait();
            }
        }

        public new ITargetLocator SwitchTo()
        {
            return new DynamicWaitingSwitchTo(this);
        }


        // ReSharper disable once MemberCanBePrivate.Global

        public void ExecuteDynamicWait()
        {
            if (_isDisposing)
            {
                return;
            }
            
            if (SkipNextWait)
            {
                SkipNextWait = false;
                return;
            }
            
            if (CurrentWindowHasBeenClosed())
            {
                return;
            }

            try
            {
                foreach (var dynamicWaitingRule in _dynamicWaitingRules)
                {
                    new WebDriverWait(_webDriver, dynamicWaitingRule.Timeout)
                        .Until(driver => ExecuteJavascriptPageFinishedLoadingCheck(driver, dynamicWaitingRule.Javascript));
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
            }
        }

        private bool CurrentWindowHasBeenClosed()
        {
            try
            {
                var _ = _webDriver.CurrentWindowHandle;
            }
            catch (Exception exception)
            {
                if (exception is NoSuchWindowException or NoSuchFrameException)
                {
                    if (DynamicWaitingSettings.AutomaticallyDetectClosedWindows)
                    {
                        SwitchToMainWindow();
                    }

                    return true;
                }
            }

            return false;
        }

        private void SwitchToMainWindow()
        {
            if (_isDisposing)
            {
                return;
            }
            
            if (WindowHandles.Count > 0)
            {
                SwitchTo().Window(OriginalWindowHandle);
                SwitchTo().DefaultContent();
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

        protected override void Dispose(bool disposing)
        {
            _isDisposing = disposing;
            base.Dispose(disposing);
        }
    }
}