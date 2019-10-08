using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter
{
    internal sealed class PreserveGeneratedFileTimestampCommand
    {
        public const int CommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("c7ea3112-3e72-418d-a66b-ec35b76962e5");
        private readonly AsyncPackage _package;

        private PreserveGeneratedFileTimestampCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
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
                preserveGeneratedFileTimestampCommand.Text = Settings.Default.PreserveGeneratedFileTimestamp ? "Do not preserve timestamp" : "Preserve timestamp";
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
