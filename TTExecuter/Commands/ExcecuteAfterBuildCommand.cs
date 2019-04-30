using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace TTExecuter.Commands
{
    internal sealed class ExcecuteAfterBuildCommand
    {
        public const int CommandId = 256;
        public static readonly Guid CommandSet = new Guid("9b56b87f-bdb0-41c1-a496-0b1a72e5b9d3");

        private readonly AsyncPackage package;

        private ExcecuteAfterBuildCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ExcecuteAfterBuildCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ExcecuteAfterBuildCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            Settings.Default.ExecuteAfterBuild = !Settings.Default.ExecuteAfterBuild;
            Settings.Default.Save();
            var command = sender as MenuCommand;
            command.Checked = Settings.Default.ExecuteAfterBuild;
        }
    }
}
