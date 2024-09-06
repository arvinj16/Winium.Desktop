namespace Winium.Desktop.Driver.CommandExecutors
{
    #region using

    using System.Threading;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Winium.Cruciatus;
    using Winium.Cruciatus.Settings;
    using Winium.Desktop.Driver.Automator;
    using Winium.Desktop.Driver.Input;
    using Winium.StoreApps.Common;

    #endregion

    internal class NewSessionExecutor : CommandExecutorBase
    {
        #region Methods

        protected override string DoImpl()
        {
            // It is easier to reparse desired capabilities as JSON instead of re-mapping keys to attributes and calling type conversions, 
            // so we will take possible one time performance hit by serializing Dictionary and deserializing it as Capabilities object
            JObject cap = 
                JObject.Parse(
                    JsonConvert.SerializeObject(
                        this.ExecutedCommand.Parameters["capabilities"]));
            var alwaysMatch = cap.SelectToken("alwaysMatch");
            this.Automator.ActualCapabilities =
                Capabilities.CapabilitiesFromJsonString(alwaysMatch.ToString());

            this.InitializeApplication(this.Automator.ActualCapabilities.DebugConnectToRunningApp);
            this.InitializeKeyboardEmulator(this.Automator.ActualCapabilities.KeyboardSimulator);

            // Gives sometime to load visuals (needed only in case of slow emulation)
            Thread.Sleep(this.Automator.ActualCapabilities.LaunchDelay);

            AutomatorResponse resp = new AutomatorResponse(this.Automator.Session, this.Automator.ActualCapabilities);
            return this.JsonResponse(ResponseStatus.Success, resp);
        }

        private void InitializeApplication(bool debugDoNotDeploy = false)
        {
            var appPath = this.Automator.ActualCapabilities.App;
            var appArguments = this.Automator.ActualCapabilities.Arguments;

            this.Automator.Application = new Application(appPath);
            if (!debugDoNotDeploy)
            {
                this.Automator.Application.Start(appArguments);
            }
        }

        private void InitializeKeyboardEmulator(KeyboardSimulatorType keyboardSimulatorType)
        {
            this.Automator.WiniumKeyboard = new WiniumKeyboard(keyboardSimulatorType);

            Logger.Debug("Current keyboard simulator: {0}", keyboardSimulatorType);
        }

        #endregion
    }

    internal class AutomatorResponse
    {
        #region Constructors and Destructors

        public AutomatorResponse(string sessionId, Capabilities capabilities)
        {
            this.SessionId = sessionId;
            this.Capabilities = capabilities;
        }
        #endregion

        #region Public Properties

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("capabilities")]
        public Capabilities Capabilities { get; set; }
        #endregion
    }
}
