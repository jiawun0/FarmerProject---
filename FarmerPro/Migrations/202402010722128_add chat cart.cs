namespace FarmerPro.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addchatcart : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Qty = c.Int(nullable: false),
                        SubTotal = c.Double(nullable: false),
                        CreateTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                        CartId = c.Int(nullable: false),
                        SpecId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cart", t => t.CartId, cascadeDelete: true)
                .ForeignKey("dbo.Spec", t => t.SpecId, cascadeDelete: true)
                .Index(t => t.CartId)
                .Index(t => t.SpecId);
            
            CreateTable(
                "dbo.Cart",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChatRoom",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserIdTalker = c.Int(nullable: false),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserIdTalker, cascadeDelete: true)
                .Index(t => t.UserIdTalker);
            
            CreateTable(
                "dbo.Record",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserIdSender = c.Int(nullable: false),
                        Message = c.String(nullable: false, maxLength: 500),
                        IsRead = c.Boolean(nullable: false),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                        ChatRoomId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatRoom", t => t.ChatRoomId, cascadeDelete: true)
                .Index(t => t.ChatRoomId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatRoom", "UserIdTalker", "dbo.Users");
            DropForeignKey("dbo.Record", "ChatRoomId", "dbo.ChatRoom");
            DropForeignKey("dbo.CartItem", "SpecId", "dbo.Spec");
            DropForeignKey("dbo.CartItem", "CartId", "dbo.Cart");
            DropIndex("dbo.Record", new[] { "ChatRoomId" });
            DropIndex("dbo.ChatRoom", new[] { "UserIdTalker" });
            DropIndex("dbo.CartItem", new[] { "SpecId" });
            DropIndex("dbo.CartItem", new[] { "CartId" });
            DropTable("dbo.Record");
            DropTable("dbo.ChatRoom");
            DropTable("dbo.Cart");
            DropTable("dbo.CartItem");
        }
    }
}
