namespace ECommerceLiteDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_product : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Description", c => c.String(nullable: false, maxLength: 500));
            DropColumn("dbo.Products", "Desctription");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "Desctription", c => c.String(nullable: false, maxLength: 500));
            DropColumn("dbo.Products", "Description");
        }
    }
}
