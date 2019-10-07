using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter
{
    internal sealed class PreserveGeneratedFileTimestampCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("b1e2aa4d-43b2-46c0-aa00-e4e3db493364");
        private readonly AsyncPackage package;

        private PreserveGeneratedFileTimestampCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.Checked = Settings.Default.PreserveGeneratedFileTimestamp;
            menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
            commandService.AddCommand(menuItem);
        }

        public static PreserveGeneratedFileTimestampCommand Instance
        {
            get;
            private set;
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand preserveGeneratedFileTimestampCommand = sender as OleMenuCommand;
            if (null != preserveGeneratedFileTimestampCommand)
            {
                preserveGeneratedFileTimestampCommand.Text = Settings.Default.PreserveGeneratedFileTimestamp ? "Disable" : "Enable";
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new PreserveGeneratedFileTimestampCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            Settings.Default.PreserveGeneratedFileTimestamp = !Settings.Default.PreserveGeneratedFileTimestamp;
            Settings.Default.Save();

            var command = sender as MenuCommand;
            command.Checked = Settings.Default.PreserveGeneratedFileTimestamp;
        }
    }
}
