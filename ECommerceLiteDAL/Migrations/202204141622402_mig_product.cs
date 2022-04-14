namespace ECommerceLiteDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mig_product : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Products", new[] { "ProductCode" });
            AddColumn("dbo.Products", "Discount", c => c.Double(nullable: false));
            AlterColumn("dbo.Products", "ProductName", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Products", "Description", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.Products", "ProductCode", c => c.String(nullable: false, maxLength: 8));
            CreateIndex("dbo.Products", "ProductCode", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Products", new[] { "ProductCode" });
            AlterColumn("dbo.Products", "ProductCode", c => c.String(maxLength: 8));
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 500));
            AlterColumn("dbo.Products", "ProductName", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.Products", "Discount");
            CreateIndex("dbo.Products", "ProductCode", unique: true);
        }
    }
}
