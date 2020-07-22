namespace TomLonghurst.Selenium.DynamicWaiting
{
    public class DynamicWaitingSettings
    {
        public bool WaitForPageLoadCompletion { get; set; }
        public bool WaitAfterScriptExecution { get; set; }
        public bool WaitAfterSwitchingTarget { get; set; }
        public bool AutomaticallyDetectClosedWindows { get; set; }

        public static DynamicWaitingSettings Default { get; } = new DynamicWaitingSettings
        {
            WaitAfterScriptExecution = true,
            WaitForPageLoadCompletion = true,
            WaitAfterSwitchingTarget = true,
            AutomaticallyDetectClosedWindows = true
        };
    }
}