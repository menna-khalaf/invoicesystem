using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication4.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "enum('أصول','خصوم','حقوق ملكية','إيرادات','مصروفات')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "countrycurrency",
                columns: table => new
                {
                    CountryCurrencyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CountryName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencySymbol = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.CountryCurrencyID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "pos_payment_methods",
                columns: table => new
                {
                    MethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MethodName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MethodType = table.Column<string>(type: "enum('Cash','CreditCard','DebitCard','MobilePayment','BankTransfer','Voucher')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.MethodID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "sitestats",
                columns: table => new
                {
                    StatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PageName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Views = table.Column<int>(type: "int", nullable: false),
                    Visitors = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.StatID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "trigger_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    trigger_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    child_account_id = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SubscriptionPlan = table.Column<string>(type: "enum('Light','Standard','Professional','Advanced')", nullable: true, defaultValueSql: "'Light'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubscriptionStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SubscriptionEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AutoRenewal = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    PaymentStatus = table.Column<string>(type: "enum('Active','Pending','Expired')", nullable: true, defaultValueSql: "'Active'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "account_type_movement",
                columns: table => new
                {
                    account_type_id = table.Column<int>(type: "int", nullable: false),
                    increase_by = table.Column<string>(type: "enum('debit','credit')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.account_type_id);
                    table.ForeignKey(
                        name: "account_type_movement_ibfk_1",
                        column: x => x.account_type_id,
                        principalTable: "account_type",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    BranchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BranchName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BranchStatus = table.Column<string>(type: "enum('نشط','غير نشط')", nullable: true, defaultValueSql: "'نشط'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Country = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MasterBranch = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    CommercialRegister = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BranchID);
                    table.ForeignKey(
                        name: "fk_branches_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactInfo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.CustomerID);
                    table.ForeignKey(
                        name: "fk_customers_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "parent_account",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_type_id = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "parent_account_ibfk_1",
                        column: x => x.account_type_id,
                        principalTable: "account_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "parent_account_ibfk_2",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "product_categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentID = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.CategoryID);
                    table.ForeignKey(
                        name: "fk_product_categories_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "product_categories_ibfk_1",
                        column: x => x.ParentID,
                        principalTable: "product_categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "costcenters",
                columns: table => new
                {
                    CostCenterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CenterName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BranchID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.CostCenterID);
                    table.ForeignKey(
                        name: "FK_CostCenter_Branch",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID");
                    table.ForeignKey(
                        name: "FK_CostCenter_User",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "storehouse",
                columns: table => new
                {
                    StorehouseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StorehouseName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubLocation = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DetailedLocation = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'"),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.StorehouseID);
                    table.ForeignKey(
                        name: "FK_Storehouse_Branches",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_storehouse_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "child_account",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_account_id = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "child_account_ibfk_1",
                        column: x => x.parent_account_id,
                        principalTable: "parent_account",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "child_account_ibfk_2",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Position = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Salary = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    AttendanceTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    DepartureTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    Role = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MobileNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecondaryMobileNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowSystemAccess = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'0'"),
                    Password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Attachments = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Branches",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_Storehouse",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_Users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    VATRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    Image = table.Column<byte[]>(type: "mediumblob", nullable: true),
                    QRCode = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RepurchasePoint = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<int>(type: "int", nullable: false),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    LastRestocked = table.Column<DateOnly>(type: "date", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_Category",
                        column: x => x.CategoryID,
                        principalTable: "product_categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_products_storehouse",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_products_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "account_mappings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    transaction_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    child_account_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "account_mappings_ibfk_1",
                        column: x => x.child_account_id,
                        principalTable: "child_account",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "childbalances",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    child_account_id = table.Column<int>(type: "int", nullable: false),
                    balance = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "childbalances_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "childbalances_ibfk_2",
                        column: x => x.child_account_id,
                        principalTable: "child_account",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "adjustments",
                columns: table => new
                {
                    AdjustmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AdjustmentDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.AdjustmentID);
                    table.ForeignKey(
                        name: "adjustments_ibfk_1",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_adjustments_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "cashandbankaccounts",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CountryCurrencyID = table.Column<int>(type: "int", nullable: true),
                    IsAccountingCodeEnabled = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    Description = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountType = table.Column<string>(type: "enum('خزينة','حساب بنكي')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "enum('نشط','غير نشط')", nullable: true, defaultValueSql: "'نشط'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ShortName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IBAN = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SwiftCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    child_account_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.AccountID);
                    table.ForeignKey(
                        name: "cashandbankaccounts_ibfk_1",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "cashandbankaccounts_ibfk_2",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_cashandbank_child_account",
                        column: x => x.child_account_id,
                        principalTable: "child_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_CashAndBankAccount_CountryCurrency",
                        column: x => x.CountryCurrencyID,
                        principalTable: "countrycurrency",
                        principalColumn: "CountryCurrencyID");
                    table.ForeignKey(
                        name: "fk_cashandbankaccounts_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "vendors",
                columns: table => new
                {
                    VendorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VendorName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber1 = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber2 = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxableCheck = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'1'"),
                    Classification = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommercialRegister = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true, defaultValueSql: "'SAR'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreditLimit = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.VendorID);
                    table.ForeignKey(
                        name: "vendors_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "vendors_ibfk_2",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderType = table.Column<string>(type: "enum('اضافه','صرف','تحويل')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.OrderID);
                    table.ForeignKey(
                        name: "fk_orders_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "orders_ibfk_1",
                        column: x => x.CustomerID,
                        principalTable: "customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "orders_ibfk_2",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "orders_ibfk_3",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "orders_ibfk_4",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "exchange",
                columns: table => new
                {
                    ExchangeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExchangeDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", maxLength: 500, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentMethod = table.Column<string>(type: "text", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExchangeType = table.Column<string>(type: "enum('صرف','قبض')", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    CostCenterID = table.Column<int>(type: "int", nullable: true),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ExchangeID);
                    table.ForeignKey(
                        name: "FK_Exchange_BankAccount",
                        column: x => x.AccountID,
                        principalTable: "cashandbankaccounts",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK_Exchange_CostCenter",
                        column: x => x.CostCenterID,
                        principalTable: "costcenters",
                        principalColumn: "CostCenterID");
                    table.ForeignKey(
                        name: "FK_Exchange_Customer",
                        column: x => x.CustomerID,
                        principalTable: "customers",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK_Exchange_User",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Exchange_Vendor",
                        column: x => x.VendorID,
                        principalTable: "vendors",
                        principalColumn: "VendorID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "purchaseorders",
                columns: table => new
                {
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    PurchaseOrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "enum('مفتوح','مغلق','ملغى')", nullable: false, defaultValueSql: "'مفتوح'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    VATRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VATAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    ItemsCount = table.Column<int>(type: "int", nullable: false),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PurchaseOrderID);
                    table.ForeignKey(
                        name: "purchaseorders_ibfk_1",
                        column: x => x.VendorID,
                        principalTable: "vendors",
                        principalColumn: "VendorID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "purchaseorders_ibfk_2",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "purchaseorders_ibfk_3",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "purchaseorders_ibfk_4",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "purchaseorders_ibfk_5",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "exchange_child_accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExchangeID = table.Column<int>(type: "int", nullable: false),
                    ChildAccountId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                    table.ForeignKey(
                        name: "exchange_child_accounts_ibfk_1",
                        column: x => x.ExchangeID,
                        principalTable: "exchange",
                        principalColumn: "ExchangeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "exchange_child_accounts_ibfk_2",
                        column: x => x.ChildAccountId,
                        principalTable: "child_account",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "journal_entry",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    entry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    debit_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    credit_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CostCenterID = table.Column<int>(type: "int", nullable: true),
                    invoiceid = table.Column<int>(type: "int", nullable: true),
                    invoicetype = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    child_account_id = table.Column<int>(type: "int", nullable: true),
                    ExchangeID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "fk_journal_child_account",
                        column: x => x.child_account_id,
                        principalTable: "child_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_journal_entry_exchange",
                        column: x => x.ExchangeID,
                        principalTable: "exchange",
                        principalColumn: "ExchangeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "journal_entry_ibfk_1",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "purchaseorderdetails",
                columns: table => new
                {
                    DetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    VatAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ProductDescription = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscountType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DetailID);
                    table.ForeignKey(
                        name: "purchaseorderdetails_ibfk_1",
                        column: x => x.PurchaseOrderID,
                        principalTable: "purchaseorders",
                        principalColumn: "PurchaseOrderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "purchaseorderdetails_ibfk_2",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "adjustmentproducts",
                columns: table => new
                {
                    AdjustmentProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AdjustmentID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    ActualQuantity = table.Column<int>(type: "int", nullable: true),
                    Difference = table.Column<int>(type: "int", nullable: true),
                    Balanced = table.Column<int>(type: "int", nullable: true),
                    InventoryID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.AdjustmentProductID);
                    table.ForeignKey(
                        name: "adjustmentproducts_ibfk_1",
                        column: x => x.AdjustmentID,
                        principalTable: "adjustments",
                        principalColumn: "AdjustmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "adjustmentproducts_ibfk_2",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "inventory",
                columns: table => new
                {
                    InventoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    IncomingQuantity = table.Column<int>(type: "int", nullable: false),
                    OutgoingQuantity = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    SessionID = table.Column<int>(type: "int", nullable: true),
                    ReferenceType = table.Column<string>(type: "enum('Invoice','Adjustment','Transfer','Order')", nullable: false, defaultValueSql: "'Invoice'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.InventoryID);
                    table.ForeignKey(
                        name: "FK_Inventory_Employee",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "fk_inventory_order",
                        column: x => x.OrderID,
                        principalTable: "orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_inventory_storehouse",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_inventory_user",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "inventory_ibfk_1",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "invoicedetails",
                columns: table => new
                {
                    DetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InvoiceID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    InvoiceTyping = table.Column<string>(type: "enum('شراء','بيع','مرتجع من الشراء','مرتجع من البيع')", nullable: true, defaultValueSql: "'بيع'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CartID = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true, comment: "References the original cart", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DetailID);
                    table.ForeignKey(
                        name: "invoicedetails_ibfk_2",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    InvoiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "enum('مدفوعة','غير مدفوعة','ملغاة')", nullable: true, defaultValueSql: "'غير مدفوعة'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentMethod = table.Column<string>(type: "enum('نقدي','تحويل بنكي','بطاقة ائتمانية','محفظة إلكترونية')", nullable: true, defaultValueSql: "'نقدي'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubscriptionPlan = table.Column<string>(type: "enum('Basic','Standard','Premium')", nullable: true, defaultValueSql: "'Basic'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    VATRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValueSql: "'15.00'"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    InvoiceType = table.Column<string>(type: "enum('شراء','بيع','مرتجع')", nullable: true, defaultValueSql: "'بيع'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    POSID = table.Column<int>(type: "int", nullable: true),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    SessionID = table.Column<int>(type: "int", nullable: true),
                    CartID = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemsCount = table.Column<int>(type: "int", nullable: true),
                    PrintedCount = table.Column<int>(type: "int", nullable: true, defaultValueSql: "'0'"),
                    PrintedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    InvoiceCashAccountID = table.Column<int>(type: "int", nullable: true),
                    InvoiceBankAccountID = table.Column<int>(type: "int", nullable: true),
                    VATAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true, computedColumnSql: "`Subtotal` + coalesce(`VATAmount`,0)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.InvoiceID);
                    table.ForeignKey(
                        name: "fk_invoice_bank_account",
                        column: x => x.InvoiceBankAccountID,
                        principalTable: "cashandbankaccounts",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "fk_invoice_branch",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_invoice_cash_account",
                        column: x => x.InvoiceCashAccountID,
                        principalTable: "cashandbankaccounts",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "fk_invoice_user",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_invoices_storehouse",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "invoices_ibfk_1",
                        column: x => x.CustomerID,
                        principalTable: "customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "invoices_ibfk_2",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "invoices_ibfk_3",
                        column: x => x.VendorID,
                        principalTable: "vendors",
                        principalColumn: "VendorID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    paydate = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    payment_status = table.Column<string>(type: "enum('Draft','Pending','Paid','Partially Paid','Cancelled')", nullable: true, defaultValueSql: "'Draft'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_method = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    InvoiceID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.payid);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices",
                        column: x => x.InvoiceID,
                        principalTable: "invoices",
                        principalColumn: "InvoiceID");
                    table.ForeignKey(
                        name: "payments_ibfk_1",
                        column: x => x.CustomerID,
                        principalTable: "customers",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "payments_ibfk_2",
                        column: x => x.VendorID,
                        principalTable: "vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "payments_ibfk_3",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "pos_transactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    InvoiceID = table.Column<int>(type: "int", nullable: true),
                    PaymentMethod = table.Column<string>(type: "enum('نقدي','تحويل بنكي','بطاقة ائتمانية','محفظة إلكترونية')", nullable: true, defaultValueSql: "'نقدي'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AmountPaid = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TransactionID);
                    table.ForeignKey(
                        name: "pos_transactions_ibfk_1",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "pos_transactions_ibfk_2",
                        column: x => x.InvoiceID,
                        principalTable: "invoices",
                        principalColumn: "InvoiceID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "quotations",
                columns: table => new
                {
                    QuotationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    QuotationDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "enum('غير مفوتر','مفوتر','ملغاة')", nullable: false, defaultValueSql: "'غير مفوتر'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    VATRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VATAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    InvoiceID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ItemsCount = table.Column<int>(type: "int", nullable: false),
                    BranchID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.QuotationID);
                    table.ForeignKey(
                        name: "quotations_ibfk_1",
                        column: x => x.CustomerID,
                        principalTable: "customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "quotations_ibfk_2",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "quotations_ibfk_3",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "quotations_ibfk_4",
                        column: x => x.InvoiceID,
                        principalTable: "invoices",
                        principalColumn: "InvoiceID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "quotations_ibfk_5",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "quotations_ibfk_6",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "transfers",
                columns: table => new
                {
                    TransferID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromAccountID = table.Column<int>(type: "int", nullable: true),
                    ToAccountID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    InvoiceID = table.Column<int>(type: "int", nullable: true),
                    TransferDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "enum('اضافة','صرف')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TransferID);
                    table.ForeignKey(
                        name: "fk_transfer_user",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transfers_fromaccount",
                        column: x => x.FromAccountID,
                        principalTable: "cashandbankaccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_transfers_toaccount",
                        column: x => x.ToAccountID,
                        principalTable: "cashandbankaccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "transfers_ibfk_3",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "transfers_ibfk_4",
                        column: x => x.InvoiceID,
                        principalTable: "invoices",
                        principalColumn: "InvoiceID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "paymentdetails",
                columns: table => new
                {
                    paydetail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    payid = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_paid = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    paid_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.paydetail_id);
                    table.ForeignKey(
                        name: "paymentdetails_ibfk_1",
                        column: x => x.payid,
                        principalTable: "payments",
                        principalColumn: "payid");
                    table.ForeignKey(
                        name: "paymentdetails_ibfk_2",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "quotationdetails",
                columns: table => new
                {
                    DetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuotationID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    ProductDescription = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DiscountType = table.Column<string>(type: "enum('percentage','amount')", nullable: false, defaultValueSql: "'amount'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Discount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    VatAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalWithVat = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DetailID);
                    table.ForeignKey(
                        name: "quotationdetails_ibfk_1",
                        column: x => x.QuotationID,
                        principalTable: "quotations",
                        principalColumn: "QuotationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quotationdetails_ibfk_2",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "pos",
                columns: table => new
                {
                    POSID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BranchID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "enum('نشطة','غير نشطة')", nullable: true, defaultValueSql: "'نشطة'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    POSName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CashAccountID = table.Column<int>(type: "int", nullable: true),
                    BankAccountID = table.Column<int>(type: "int", nullable: true),
                    StorehouseID = table.Column<int>(type: "int", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Attachments = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SessionID = table.Column<int>(type: "int", nullable: true),
                    CartID = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.POSID);
                    table.ForeignKey(
                        name: "fk_pos_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "pos_ibfk_1",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "pos_ibfk_2",
                        column: x => x.CashAccountID,
                        principalTable: "cashandbankaccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "pos_ibfk_3",
                        column: x => x.BankAccountID,
                        principalTable: "cashandbankaccounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "pos_ibfk_4",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "pos_sessions",
                columns: table => new
                {
                    SessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    POSID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: true),
                    StorehouseID = table.Column<int>(type: "int", nullable: false),
                    BranchID = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    StartingCash = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    EndingCash = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "enum('Active','Closed','Suspended')", nullable: true, defaultValueSql: "'Active'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.SessionID);
                    table.ForeignKey(
                        name: "fk_pos_sessions_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "pos_sessions_ibfk_1",
                        column: x => x.POSID,
                        principalTable: "pos",
                        principalColumn: "POSID");
                    table.ForeignKey(
                        name: "pos_sessions_ibfk_2",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "pos_sessions_ibfk_3",
                        column: x => x.StorehouseID,
                        principalTable: "storehouse",
                        principalColumn: "StorehouseID");
                    table.ForeignKey(
                        name: "pos_sessions_ibfk_4",
                        column: x => x.BranchID,
                        principalTable: "branches",
                        principalColumn: "BranchID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "shifts",
                columns: table => new
                {
                    ShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StartDateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    ShiftHours = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    POSID = table.Column<int>(type: "int", nullable: false),
                    RepeatWeekly = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RepeatDays = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ShiftID);
                    table.ForeignKey(
                        name: "fk_shifts_users",
                        column: x => x.UserID,
                        principalTable: "users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "shifts_ibfk_1",
                        column: x => x.EmployeeID,
                        principalTable: "employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "shifts_ibfk_2",
                        column: x => x.POSID,
                        principalTable: "pos",
                        principalColumn: "POSID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "pos_carts",
                columns: table => new
                {
                    CartID = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    SessionID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "'1'"),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, defaultValueSql: "'0.00'"),
                    AddedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "enum('Active','Completed','Cancelled')", nullable: true, defaultValueSql: "'Active'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.CartID, x.ProductID })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "pos_carts_ibfk_1",
                        column: x => x.SessionID,
                        principalTable: "pos_sessions",
                        principalColumn: "SessionID");
                    table.ForeignKey(
                        name: "pos_carts_ibfk_2",
                        column: x => x.ProductID,
                        principalTable: "products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "child_account_id",
                table: "account_mappings",
                column: "child_account_id");

            migrationBuilder.CreateIndex(
                name: "AdjustmentID",
                table: "adjustmentproducts",
                column: "AdjustmentID");

            migrationBuilder.CreateIndex(
                name: "InventoryID",
                table: "adjustmentproducts",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "ProductID",
                table: "adjustmentproducts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID",
                table: "adjustments",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "fk_adjustments_users",
                table: "adjustments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "fk_branches_users",
                table: "branches",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "AccountNumber",
                table: "cashandbankaccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "BranchID",
                table: "cashandbankaccounts",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID1",
                table: "cashandbankaccounts",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "fk_cashandbank_child_account",
                table: "cashandbankaccounts",
                column: "child_account_id");

            migrationBuilder.CreateIndex(
                name: "fk_CashAndBankAccount_CountryCurrency",
                table: "cashandbankaccounts",
                column: "CountryCurrencyID");

            migrationBuilder.CreateIndex(
                name: "fk_cashandbankaccounts_users",
                table: "cashandbankaccounts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "parent_account_id",
                table: "child_account",
                column: "parent_account_id");

            migrationBuilder.CreateIndex(
                name: "unique_child_account_name",
                table: "child_account",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserID",
                table: "child_account",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "child_account_id1",
                table: "childbalances",
                column: "child_account_id");

            migrationBuilder.CreateIndex(
                name: "idx_childbalances_userid_childaccountid",
                table: "childbalances",
                columns: new[] { "UserID", "child_account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "FK_CostCenter_Branch",
                table: "costcenters",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "FK_CostCenter_User",
                table: "costcenters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "fk_customers_users",
                table: "customers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "FK_Employees_Branches",
                table: "employees",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "FK_Employees_Storehouse",
                table: "employees",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "FK_Employees_Users",
                table: "employees",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "FK_Exchange_BankAccount",
                table: "exchange",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "FK_Exchange_CostCenter",
                table: "exchange",
                column: "CostCenterID");

            migrationBuilder.CreateIndex(
                name: "FK_Exchange_Customer",
                table: "exchange",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "FK_Exchange_User",
                table: "exchange",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "FK_Exchange_Vendor",
                table: "exchange",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "ChildAccountId",
                table: "exchange_child_accounts",
                column: "ChildAccountId");

            migrationBuilder.CreateIndex(
                name: "idx_exchange_child_accounts_exchangeid",
                table: "exchange_child_accounts",
                column: "ExchangeID");

            migrationBuilder.CreateIndex(
                name: "FK_Inventory_Employee",
                table: "inventory",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "fk_inventory_order",
                table: "inventory",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "FK_Inventory_Session",
                table: "inventory",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "fk_inventory_storehouse",
                table: "inventory",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "fk_inventory_user",
                table: "inventory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "ProductID1",
                table: "inventory",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "FK_InvoiceDetails_Cart",
                table: "invoicedetails",
                column: "CartID");

            migrationBuilder.CreateIndex(
                name: "InvoiceID",
                table: "invoicedetails",
                column: "InvoiceID");

            migrationBuilder.CreateIndex(
                name: "ProductID2",
                table: "invoicedetails",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "CustomerID",
                table: "invoices",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID2",
                table: "invoices",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "fk_invoice_bank_account",
                table: "invoices",
                column: "InvoiceBankAccountID");

            migrationBuilder.CreateIndex(
                name: "fk_invoice_branch",
                table: "invoices",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "fk_invoice_cash_account",
                table: "invoices",
                column: "InvoiceCashAccountID");

            migrationBuilder.CreateIndex(
                name: "fk_invoice_user",
                table: "invoices",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "FK_Invoices_Cart",
                table: "invoices",
                column: "CartID");

            migrationBuilder.CreateIndex(
                name: "FK_Invoices_POS",
                table: "invoices",
                column: "POSID");

            migrationBuilder.CreateIndex(
                name: "FK_Invoices_Session",
                table: "invoices",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "fk_invoices_storehouse",
                table: "invoices",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "VendorID",
                table: "invoices",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "fk_costcenter",
                table: "journal_entry",
                column: "CostCenterID");

            migrationBuilder.CreateIndex(
                name: "fk_journal_child_account",
                table: "journal_entry",
                column: "child_account_id");

            migrationBuilder.CreateIndex(
                name: "idx_journal_entry_exchangeid",
                table: "journal_entry",
                column: "ExchangeID");

            migrationBuilder.CreateIndex(
                name: "UserID1",
                table: "journal_entry",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "CustomerID1",
                table: "orders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID3",
                table: "orders",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "fk_orders_users",
                table: "orders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "ProductID3",
                table: "orders",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "StorehouseID",
                table: "orders",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "account_type_id",
                table: "parent_account",
                column: "account_type_id");

            migrationBuilder.CreateIndex(
                name: "UserID2",
                table: "parent_account",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "payid",
                table: "paymentdetails",
                column: "payid");

            migrationBuilder.CreateIndex(
                name: "UserID4",
                table: "paymentdetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "CustomerID2",
                table: "payments",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "FK_Payments_Invoices",
                table: "payments",
                column: "InvoiceID");

            migrationBuilder.CreateIndex(
                name: "UserID3",
                table: "payments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "VendorID1",
                table: "payments",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "BankAccountID",
                table: "pos",
                column: "BankAccountID");

            migrationBuilder.CreateIndex(
                name: "BranchID1",
                table: "pos",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "CashAccountID",
                table: "pos",
                column: "CashAccountID");

            migrationBuilder.CreateIndex(
                name: "fk_pos_users",
                table: "pos",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "FK_Transactions_Cart",
                table: "pos",
                column: "CartID");

            migrationBuilder.CreateIndex(
                name: "FK_Transactions_Session",
                table: "pos",
                column: "SessionID");

            migrationBuilder.CreateIndex(
                name: "StorehouseID1",
                table: "pos",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "idx_cart_product",
                table: "pos_carts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "idx_cart_session",
                table: "pos_carts",
                columns: new[] { "SessionID", "Status" });

            migrationBuilder.CreateIndex(
                name: "BranchID2",
                table: "pos_sessions",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "fk_pos_sessions_users",
                table: "pos_sessions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "idx_session_employee",
                table: "pos_sessions",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "idx_session_status",
                table: "pos_sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "POSID",
                table: "pos_sessions",
                column: "POSID");

            migrationBuilder.CreateIndex(
                name: "StorehouseID2",
                table: "pos_sessions",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID4",
                table: "pos_transactions",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "InvoiceID1",
                table: "pos_transactions",
                column: "InvoiceID");

            migrationBuilder.CreateIndex(
                name: "fk_product_categories_users",
                table: "product_categories",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "ParentID",
                table: "product_categories",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "FK_Products_Category",
                table: "products",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "fk_products_storehouse",
                table: "products",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "fk_products_users",
                table: "products",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "ProductID4",
                table: "purchaseorderdetails",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "PurchaseOrderID",
                table: "purchaseorderdetails",
                column: "PurchaseOrderID");

            migrationBuilder.CreateIndex(
                name: "BranchID3",
                table: "purchaseorders",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID5",
                table: "purchaseorders",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "StorehouseID3",
                table: "purchaseorders",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "UserID5",
                table: "purchaseorders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "VendorID2",
                table: "purchaseorders",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "ProductID5",
                table: "quotationdetails",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "QuotationID",
                table: "quotationdetails",
                column: "QuotationID");

            migrationBuilder.CreateIndex(
                name: "BranchID4",
                table: "quotations",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "CustomerID3",
                table: "quotations",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID6",
                table: "quotations",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "InvoiceID2",
                table: "quotations",
                column: "InvoiceID");

            migrationBuilder.CreateIndex(
                name: "StorehouseID4",
                table: "quotations",
                column: "StorehouseID");

            migrationBuilder.CreateIndex(
                name: "UserID6",
                table: "quotations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID7",
                table: "shifts",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "fk_shifts_users",
                table: "shifts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "POSID1",
                table: "shifts",
                column: "POSID");

            migrationBuilder.CreateIndex(
                name: "FK_Storehouse_Branches",
                table: "storehouse",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "fk_storehouse_users",
                table: "storehouse",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "EmployeeID8",
                table: "transfers",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "fk_transfer_user",
                table: "transfers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "fk_transfers_fromaccount",
                table: "transfers",
                column: "FromAccountID");

            migrationBuilder.CreateIndex(
                name: "fk_transfers_toaccount",
                table: "transfers",
                column: "ToAccountID");

            migrationBuilder.CreateIndex(
                name: "InvoiceID3",
                table: "transfers",
                column: "InvoiceID");

            migrationBuilder.CreateIndex(
                name: "Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Username",
                table: "users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmployeeID9",
                table: "vendors",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "UserID7",
                table: "vendors",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "adjustmentproducts_ibfk_3",
                table: "adjustmentproducts",
                column: "InventoryID",
                principalTable: "inventory",
                principalColumn: "InventoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Session",
                table: "inventory",
                column: "SessionID",
                principalTable: "pos_sessions",
                principalColumn: "SessionID");

            migrationBuilder.AddForeignKey(
                name: "invoicedetails_ibfk_1",
                table: "invoicedetails",
                column: "InvoiceID",
                principalTable: "invoices",
                principalColumn: "InvoiceID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_POS",
                table: "invoices",
                column: "POSID",
                principalTable: "pos",
                principalColumn: "POSID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Session",
                table: "invoices",
                column: "SessionID",
                principalTable: "pos_sessions",
                principalColumn: "SessionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Session",
                table: "pos",
                column: "SessionID",
                principalTable: "pos_sessions",
                principalColumn: "SessionID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cashandbank_child_account",
                table: "cashandbankaccounts");

            migrationBuilder.DropForeignKey(
                name: "cashandbankaccounts_ibfk_1",
                table: "cashandbankaccounts");

            migrationBuilder.DropForeignKey(
                name: "pos_sessions_ibfk_2",
                table: "pos_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_branches_users",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "fk_cashandbankaccounts_users",
                table: "cashandbankaccounts");

            migrationBuilder.DropForeignKey(
                name: "fk_pos_users",
                table: "pos");

            migrationBuilder.DropForeignKey(
                name: "fk_pos_sessions_users",
                table: "pos_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_storehouse_users",
                table: "storehouse");

            migrationBuilder.DropForeignKey(
                name: "cashandbankaccounts_ibfk_2",
                table: "cashandbankaccounts");

            migrationBuilder.DropForeignKey(
                name: "pos_ibfk_1",
                table: "pos");

            migrationBuilder.DropForeignKey(
                name: "pos_sessions_ibfk_4",
                table: "pos_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Storehouse_Branches",
                table: "storehouse");

            migrationBuilder.DropForeignKey(
                name: "fk_CashAndBankAccount_CountryCurrency",
                table: "cashandbankaccounts");

            migrationBuilder.DropForeignKey(
                name: "pos_ibfk_4",
                table: "pos");

            migrationBuilder.DropForeignKey(
                name: "pos_sessions_ibfk_3",
                table: "pos_sessions");

            migrationBuilder.DropForeignKey(
                name: "pos_ibfk_2",
                table: "pos");

            migrationBuilder.DropForeignKey(
                name: "pos_ibfk_3",
                table: "pos");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Session",
                table: "pos");

            migrationBuilder.DropTable(
                name: "account_mappings");

            migrationBuilder.DropTable(
                name: "account_type_movement");

            migrationBuilder.DropTable(
                name: "adjustmentproducts");

            migrationBuilder.DropTable(
                name: "childbalances");

            migrationBuilder.DropTable(
                name: "exchange_child_accounts");

            migrationBuilder.DropTable(
                name: "invoicedetails");

            migrationBuilder.DropTable(
                name: "journal_entry");

            migrationBuilder.DropTable(
                name: "paymentdetails");

            migrationBuilder.DropTable(
                name: "pos_carts");

            migrationBuilder.DropTable(
                name: "pos_payment_methods");

            migrationBuilder.DropTable(
                name: "pos_transactions");

            migrationBuilder.DropTable(
                name: "purchaseorderdetails");

            migrationBuilder.DropTable(
                name: "quotationdetails");

            migrationBuilder.DropTable(
                name: "shifts");

            migrationBuilder.DropTable(
                name: "sitestats");

            migrationBuilder.DropTable(
                name: "transfers");

            migrationBuilder.DropTable(
                name: "trigger_log");

            migrationBuilder.DropTable(
                name: "adjustments");

            migrationBuilder.DropTable(
                name: "inventory");

            migrationBuilder.DropTable(
                name: "exchange");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "purchaseorders");

            migrationBuilder.DropTable(
                name: "quotations");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "costcenters");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "vendors");

            migrationBuilder.DropTable(
                name: "product_categories");

            migrationBuilder.DropTable(
                name: "child_account");

            migrationBuilder.DropTable(
                name: "parent_account");

            migrationBuilder.DropTable(
                name: "account_type");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "countrycurrency");

            migrationBuilder.DropTable(
                name: "storehouse");

            migrationBuilder.DropTable(
                name: "cashandbankaccounts");

            migrationBuilder.DropTable(
                name: "pos_sessions");

            migrationBuilder.DropTable(
                name: "pos");
        }
    }
}
