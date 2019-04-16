using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter
{
    internal sealed class EnableDisableTTExecuterCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("c7ea3112-3e72-418d-a66b-ec35b76962e5");
        private readonly AsyncPackage package;

        private EnableDisableTTExecuterCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            menuItem.Checked = Settings.Default.EnableTTExecuter;
            commandService.AddCommand(menuItem);
        }

        public static EnableDisableTTExecuterCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new EnableDisableTTExecuterCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            Settings.Default.EnableTTExecuter = !Settings.Default.EnableTTExecuter;
            Settings.Default.Save();
            var command = sender as MenuCommand;
            command.Checked = Settings.Default.EnableTTExecuter;
        }
    }
}
