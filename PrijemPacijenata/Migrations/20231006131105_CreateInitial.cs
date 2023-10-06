using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrijemPacijenata.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dijagnoze",
                columns: table => new
                {
                    IDDijagnoze = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImeDijagnoze = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dijagnoze", x => x.IDDijagnoze);
                });

            migrationBuilder.CreateTable(
                name: "Doktori",
                columns: table => new
                {
                    IDDoktora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImeDoktora = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrezimeDoktora = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doktori", x => x.IDDoktora);
                });

            migrationBuilder.CreateTable(
                name: "Pacijenti",
                columns: table => new
                {
                    IDPacijenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrojSobe = table.Column<int>(type: "int", nullable: true),
                    DoktorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacijenti", x => x.IDPacijenta);
                    table.ForeignKey(
                        name: "FK_Pacijenti_Doktori_DoktorId",
                        column: x => x.DoktorId,
                        principalTable: "Doktori",
                        principalColumn: "IDDoktora");
                });

            migrationBuilder.CreateTable(
                name: "DijagnozaPacijent",
                columns: table => new
                {
                    DijagnozeIDDijagnoze = table.Column<int>(type: "int", nullable: false),
                    PacijentsIDPacijenta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DijagnozaPacijent", x => new { x.DijagnozeIDDijagnoze, x.PacijentsIDPacijenta });
                    table.ForeignKey(
                        name: "FK_DijagnozaPacijent_Dijagnoze_DijagnozeIDDijagnoze",
                        column: x => x.DijagnozeIDDijagnoze,
                        principalTable: "Dijagnoze",
                        principalColumn: "IDDijagnoze",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DijagnozaPacijent_Pacijenti_PacijentsIDPacijenta",
                        column: x => x.PacijentsIDPacijenta,
                        principalTable: "Pacijenti",
                        principalColumn: "IDPacijenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DijagnozaPacijent_PacijentsIDPacijenta",
                table: "DijagnozaPacijent",
                column: "PacijentsIDPacijenta");

            migrationBuilder.CreateIndex(
                name: "IX_Pacijenti_DoktorId",
                table: "Pacijenti",
                column: "DoktorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DijagnozaPacijent");

            migrationBuilder.DropTable(
                name: "Dijagnoze");

            migrationBuilder.DropTable(
                name: "Pacijenti");

            migrationBuilder.DropTable(
                name: "Doktori");
        }
    }
}
