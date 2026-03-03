using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkloadsModule.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workload_customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workload_customers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workload_employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workload_employees", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workloads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    stop_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    comment = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    customer_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    employee_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workloads", x => x.id);
                    table.ForeignKey(
                        name: "FK_workloads_workload_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "workload_customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workloads_workload_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "workload_employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_workload_customers_email",
                table: "workload_customers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_workload_employees_email",
                table: "workload_employees",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_workloads_customer_id",
                table: "workloads",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_workloads_employee_id",
                table: "workloads",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_workloads_start_date",
                table: "workloads",
                column: "start_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workloads");

            migrationBuilder.DropTable(
                name: "workload_customers");

            migrationBuilder.DropTable(
                name: "workload_employees");
        }
    }
}
