namespace FarmerPro.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addspecsale : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Spec", "Sales", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Spec", "Sales");
        }
    }
}
