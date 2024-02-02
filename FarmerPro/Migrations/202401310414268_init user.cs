namespace FarmerPro.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class inituser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category = c.Int(nullable: false),
                        Account = c.String(nullable: false, maxLength: 500),
                        EmailGUID = c.Guid(),
                        Password = c.String(nullable: false, maxLength: 500),
                        Salt = c.String(nullable: false, maxLength: 500),
                        Token = c.String(maxLength: 500),
                        NickName = c.String(maxLength: 500),
                        Photo = c.String(),
                        Birthday = c.DateTime(),
                        Sex = c.Boolean(),
                        Phone = c.String(maxLength: 100),
                        Vision = c.String(),
                        Description = c.String(),
                        CreatTime = c.DateTime(nullable: false, defaultValueSql: "getdate()"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
        }
    }
}
