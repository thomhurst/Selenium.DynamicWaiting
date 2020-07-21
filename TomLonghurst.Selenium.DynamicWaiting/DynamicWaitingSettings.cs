namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class DynamicWaitingSettings
    {
        public bool WaitForPageLoadCompletion { get; set; }
        public bool WaitAfterScriptExecution { get; set; }
        public bool WaitAfterSwitchingWindow { get; set; }
    }
}