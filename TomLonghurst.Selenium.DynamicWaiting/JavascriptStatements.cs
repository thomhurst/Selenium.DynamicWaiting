namespace TomLonghurst.Selenium.DynamicWaiting
{
    internal static class JavascriptStatements
    {
        internal static string DocumentReadyStateComplete = "return document.readyState == \"complete\"";
        internal static string JQuery = "return window.jQuery == null || jQuery.active == 0";
    }
}