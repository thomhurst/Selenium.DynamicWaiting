namespace TomLonghurst.Selenium.DynamicWaiting
{
    internal static class JavascriptStatements
    {
        internal static readonly string DocumentReadyStateComplete = "return document.readyState == \"complete\"";
        internal static readonly string JQuery = "return window.jQuery == null || jQuery.active == 0";
    }
}