namespace pscommander
{
    public class CommandService
    {
        private readonly FileAssociationService _fileAssociationService;
        private readonly ShortcutService _shortcutService;
        private readonly ContextMenuService _contextMenuService;
        private readonly CustomProtocolService _customProtocolService;

        public CommandService(FileAssociationService fileAssociationService, ShortcutService shortcutService, ContextMenuService contextMenuService, CustomProtocolService customProtocolService)
        {
            _fileAssociationService = fileAssociationService;
            _shortcutService = shortcutService;
            _contextMenuService = contextMenuService;
            _customProtocolService = customProtocolService;
        }

        public void ProcessCommand(Command command)
        {
            switch(command.Name)
            {
                case "fileAssociation":
                    ProcessFileAssociation(command);
                    break;
                case "shortcut":
                    ProcessShortcut(command);
                    break;
                case "contextMenu":
                    ProcessContextMenu(command);
                    break;
                case "protocol":
                    ProcessProtocol(command);
                    break;
            }
        }

        private void ProcessProtocol(Command command)
        {
            if (!command.Properties.ContainsKey("protocol")) return;
            var protocol = command.Properties["protocol"];
            var protocolArg = command.Properties["arg"];
            _customProtocolService.ExecuteProtocol(protocol, protocolArg);
        }


        private void ProcessShortcut(Command command)
        {
            if (!command.Properties.ContainsKey("id")) return;
            var id = command.Properties["id"];
            _shortcutService.Execute(int.Parse(id));
        }

        private void ProcessFileAssociation(Command command)
        {
            if (!command.Properties.ContainsKey("filePath")) return;
            var filePath = command.Properties["filePath"];
            _fileAssociationService.ExecuteAssociation(filePath);
        }

        private void ProcessContextMenu(Command command)
        {
            if (!command.Properties.ContainsKey("path")) return;
            if (!command.Properties.ContainsKey("id")) return;
            var path = command.Properties["path"];
            var id = command.Properties["id"];
            _contextMenuService.ExecuteMenuItem(int.Parse(id), path);
        }
    }
}