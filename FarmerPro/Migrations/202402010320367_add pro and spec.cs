namespace FarmerPro.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addproandspec : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Product",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductTitle = c.String(nullable: false, maxLength: 500),
                        Category = c.Int(nullable: false),
                        Period = c.Int(nullable: false),
                        Origin = c.Int(nullable: false),
                        Storage = c.Int(nullable: false),
                        Description = c.String(maxLength: 500),
                        Introduction = c.String(),
                        ProductState = c.Boolean(nullable: false),
                        UpdateStateTime = c.DateTime(),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Spec",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Price = c.Int(nullable: false),
                        Stock = c.Int(nullable: false),
                        PromotePrice = c.Int(),
                        LivePrice = c.Int(),
                        Size = c.Boolean(nullable: false),
                        Weight = c.Double(nullable: false),
                        CreateTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Product", "UserId", "dbo.Users");
            DropForeignKey("dbo.Spec", "ProductId", "dbo.Product");
            DropIndex("dbo.Spec", new[] { "ProductId" });
            DropIndex("dbo.Product", new[] { "UserId" });
            DropTable("dbo.Spec");
            DropTable("dbo.Product");
        }
    }
}
