using System;
using System.IO;
using LiteDB;

namespace pscommander
{
    public class DataService
    {
        private readonly LiteDatabase _database;

        public DataService()
        {
            BsonMapper.Global.Entity<FileAssociation>().Ignore(m => m.Action);     
            BsonMapper.Global.Entity<Shortcut>().Ignore(m => m.Action);
            BsonMapper.Global.Entity<ExplorerContextMenu>().Ignore(m => m.Action);
            BsonMapper.Global.Entity<CustomProtocol>().Ignore(m => m.Action);

            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PSCommander");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            var database = Path.Combine(dataDirectory, "database.db");

            _database = new LiteDatabase(database);
        }

        public ILiteCollection<CustomProtocol> CustomProtocols => _database.GetCollection<CustomProtocol>("customProtocols");
        public ILiteCollection<Shortcut> Shortcuts => _database.GetCollection<Shortcut>("shortcuts");
        public ILiteCollection<FileAssociation> FileAssociations => _database.GetCollection<FileAssociation>("fileAssociations");
        public ILiteCollection<ExplorerContextMenu> ExplorerContextMenus => _database.GetCollection<ExplorerContextMenu>("contextMenus");


    }
}