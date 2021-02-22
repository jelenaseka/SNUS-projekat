namespace ScadaCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class change2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TagValues", "Type", c => c.String());
            AddColumn("dbo.TagValues", "Tag", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TagValues", "Tag");
            DropColumn("dbo.TagValues", "Type");
        }
    }
}
