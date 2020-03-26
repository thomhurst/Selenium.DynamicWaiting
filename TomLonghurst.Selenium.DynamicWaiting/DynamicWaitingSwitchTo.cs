using System;
using OpenQA.Selenium;

namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class DynamicWaitingSwitchTo : ITargetLocator
    {
        private readonly DynamicWaitingWebDriver _dynamicWaitingWebDriver;
        
        public DynamicWaitingSwitchTo(DynamicWaitingWebDriver dynamicWaitingWebDriver)
        {
            _dynamicWaitingWebDriver = dynamicWaitingWebDriver;
        }
        
        public IWebDriver Frame(int frameIndex)
        {
            return SwitchAndWait(switchTo => switchTo.Frame(frameIndex));
        }

        public IWebDriver Frame(string frameName)
        {
            return SwitchAndWait(switchTo => switchTo.Frame(frameName));
        }

        public IWebDriver Frame(IWebElement frameElement)
        {
            return SwitchAndWait(switchTo => switchTo.Frame(frameElement));
        }

        public IWebDriver ParentFrame()
        {
            return SwitchAndWait(switchTo => switchTo.ParentFrame());
        }

        public IWebDriver Window(string windowName)
        {
            return SwitchAndWait(switchTo => switchTo.Window(windowName));
        }

        public IWebDriver DefaultContent()
        {
            return SwitchAndWait(switchTo => switchTo.DefaultContent());
        }

        public IWebElement ActiveElement()
        {
            return SwitchAndWait(switchTo => switchTo.ActiveElement());
        }

        public IAlert Alert()
        {
            return SwitchAndWait(switchTo => switchTo.Alert());
        }

        private T SwitchAndWait<T>(Func<ITargetLocator, T> action)
        {
            _dynamicWaitingWebDriver.ExecuteDynamicWait();
            var result = action(_dynamicWaitingWebDriver.SwitchTo());
            _dynamicWaitingWebDriver.ExecuteDynamicWait();
            return result;
        }
    }
}