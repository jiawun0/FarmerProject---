namespace FarmerPro.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLiveSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Album",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Photo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        URL = c.String(nullable: false),
                        AlbumId = c.Int(nullable: false),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Album", t => t.AlbumId, cascadeDelete: true)
                .Index(t => t.AlbumId);
            
            CreateTable(
                "dbo.LiveProduct",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsTop = c.Boolean(nullable: false),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                        LiveSettingId = c.Int(nullable: false),
                        SpecId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LiveSetting", t => t.LiveSettingId, cascadeDelete: true)
                .ForeignKey("dbo.Spec", t => t.SpecId, cascadeDelete: true)
                .Index(t => t.LiveSettingId)
                .Index(t => t.SpecId);
            
            CreateTable(
                "dbo.LiveSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LiveName = c.String(nullable: false, maxLength: 500),
                        LiveDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        LivePic = c.String(),
                        YTURL = c.String(nullable: false),
                        ShareURL = c.String(nullable: false),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OrderDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Qty = c.Int(nullable: false),
                        SubTotal = c.Double(nullable: false),
                        CreatTime = c.DateTime(nullable: false , defaultValueSql: "getdate()"),
                        SpecId = c.Int(nullable: false),
                        OrderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Spec", t => t.SpecId, cascadeDelete: true)
                .Index(t => t.SpecId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Order",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Receiver = c.String(nullable: false, maxLength: 100),
                        Photo = c.String(nullable: false, maxLength: 100),
                        City = c.Int(nullable: false),
                        District = c.String(nullable: false),
                        ZipCode = c.Int(nullable: false),
                        Address = c.String(nullable: false, maxLength: 300),
                        DeliveryFee = c.Double(nullable: false),
                        OrderSum = c.Double(nullable: false),
                        Shipment = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                        PaymentTime = c.DateTime(),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LiveProduct", "SpecId", "dbo.Spec");
            DropForeignKey("dbo.OrderDetail", "SpecId", "dbo.Spec");
            DropForeignKey("dbo.OrderDetail", "OrderId", "dbo.Order");
            DropForeignKey("dbo.LiveProduct", "LiveSettingId", "dbo.LiveSetting");
            DropForeignKey("dbo.Photo", "AlbumId", "dbo.Album");
            DropIndex("dbo.OrderDetail", new[] { "OrderId" });
            DropIndex("dbo.OrderDetail", new[] { "SpecId" });
            DropIndex("dbo.LiveProduct", new[] { "SpecId" });
            DropIndex("dbo.LiveProduct", new[] { "LiveSettingId" });
            DropIndex("dbo.Photo", new[] { "AlbumId" });
            DropTable("dbo.Order");
            DropTable("dbo.OrderDetail");
            DropTable("dbo.LiveSetting");
            DropTable("dbo.LiveProduct");
            DropTable("dbo.Photo");
            DropTable("dbo.Album");
        }
    }
}
