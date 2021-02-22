namespace ScadaCore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAlarms : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alarms",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Type = c.String(),
                        Priority = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                        TagName = c.String(),
                        Limit = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Alarms");
        }
    }
}
