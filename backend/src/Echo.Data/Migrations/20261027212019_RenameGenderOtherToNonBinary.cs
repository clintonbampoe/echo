using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echo.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameGenderOtherToNonBinary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Genders are stored as text, so the renamed value has to be rewritten in place.
            migrationBuilder.Sql("UPDATE members SET gender = 'NonBinary' WHERE gender = 'Other';");
            migrationBuilder.Sql("UPDATE visitors SET gender = 'NonBinary' WHERE gender = 'Other';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE members SET gender = 'Other' WHERE gender = 'NonBinary';");
            migrationBuilder.Sql("UPDATE visitors SET gender = 'Other' WHERE gender = 'NonBinary';");
        }
    }
}
