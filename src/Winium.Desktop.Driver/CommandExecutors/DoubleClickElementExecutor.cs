namespace Winium.Desktop.Driver.CommandExecutors
{
    internal class DoubleClickElementExecutor : CommandExecutorBase
    {
        #region Methods

        protected override string DoImpl()
        {
            var registeredKey = this.ExecutedCommand.Parameters["ID"].ToString();
            this.Automator.ElementsRegistry.GetRegisteredElement(registeredKey).DoubleClick();

            return this.JsonResponse();
        }

        #endregion
    }
}
