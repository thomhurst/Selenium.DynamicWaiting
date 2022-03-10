using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TomLonghurst.Selenium.DynamicWaiting.Tests
{
    public class Tests
    {
        private DynamicWaitingWebDriver _dynamicWaitingWebDriver;

        [SetUp]
        public void Setup()
        {
             var baseWebDriver = new ChromeDriver();
             _dynamicWaitingWebDriver = new DynamicWaitingWebDriver(baseWebDriver, new []
             {
                 DynamicWaitingRule.JQuery(TimeSpan.FromSeconds(30)), 
             });
        }

        [Test]
        public void Test1()
        {
            _dynamicWaitingWebDriver.Navigate().GoToUrl("https://my.asos.com");
            var emailAddressBox = _dynamicWaitingWebDriver.FindElement(By.Id("EmailAddress"));
            emailAddressBox.SendKeys("test email input");
            
            var passwordBox = _dynamicWaitingWebDriver.FindElement(By.Id("Password"));
            passwordBox.SendKeys("test password input");
            
            var signInButton = _dynamicWaitingWebDriver.FindElement(By.Id("signin"));
            signInButton.Click();
            
            Thread.Sleep(15000);
        }
    }
}