using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MechanicalDesigns.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260921090000_AddGovernorateDeliveryFees")]
public partial class AddGovernorateDeliveryFees : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GovernorateDeliveryFees",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                DeliveryFee = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GovernorateDeliveryFees", x => x.Id);
                table.CheckConstraint("CK_GovernorateDeliveryFees_Fee", "\"DeliveryFee\" IS NULL OR \"DeliveryFee\" >= 0");
            });

        migrationBuilder.CreateIndex(name: "IX_GovernorateDeliveryFees_Name", table: "GovernorateDeliveryFees", column: "Name", unique: true);

        var createdAt = new DateTimeOffset(2026, 9, 21, 0, 0, 0, TimeSpan.Zero);
        migrationBuilder.InsertData(
            table: "GovernorateDeliveryFees",
            columns: new[] { "Id", "Name", "NameAr", "DeliveryFee", "IsActive", "CreatedAt", "UpdatedAt" },
            columnTypes: new[] { "uuid", "character varying(100)", "character varying(100)", "numeric(12,2)", "boolean", "timestamp with time zone", "timestamp with time zone" },
            values: new object[,]
            {
                { new Guid("10000000-0000-0000-0000-000000000001"), "Alexandria", "الإسكندرية", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000002"), "Aswan", "أسوان", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000003"), "Asyut", "أسيوط", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000004"), "Beheira", "البحيرة", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000005"), "Beni Suef", "بني سويف", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000006"), "Cairo", "القاهرة", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000007"), "Dakahlia", "الدقهلية", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000008"), "Damietta", "دمياط", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000009"), "Faiyum", "الفيوم", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000010"), "Gharbia", "الغربية", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000011"), "Giza", "الجيزة", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000012"), "Ismailia", "الإسماعيلية", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000013"), "Kafr El Sheikh", "كفر الشيخ", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000014"), "Luxor", "الأقصر", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000015"), "Matrouh", "مطروح", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000016"), "Minya", "المنيا", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000017"), "Monufia", "المنوفية", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000018"), "New Valley", "الوادي الجديد", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000019"), "North Sinai", "شمال سيناء", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000020"), "Port Said", "بورسعيد", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000021"), "Qalyubia", "القليوبية", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000022"), "Qena", "قنا", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000023"), "Red Sea", "البحر الأحمر", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000024"), "Sharqia", "الشرقية", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000025"), "Sohag", "سوهاج", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000026"), "South Sinai", "جنوب سيناء", null, true, createdAt, createdAt },
                { new Guid("10000000-0000-0000-0000-000000000027"), "Suez", "السويس", null, true, createdAt, createdAt }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "GovernorateDeliveryFees");
}
