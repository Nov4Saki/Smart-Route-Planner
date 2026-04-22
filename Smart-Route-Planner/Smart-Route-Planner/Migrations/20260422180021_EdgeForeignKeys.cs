using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Route_Planner.Migrations
{
    /// <inheritdoc />
    public partial class EdgeForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Edges_FromNodeId",
                table: "Edges",
                column: "FromNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_ToNodeId",
                table: "Edges",
                column: "ToNodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_Nodes_FromNodeId",
                table: "Edges",
                column: "FromNodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_Nodes_ToNodeId",
                table: "Edges",
                column: "ToNodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Edges_Nodes_FromNodeId",
                table: "Edges");

            migrationBuilder.DropForeignKey(
                name: "FK_Edges_Nodes_ToNodeId",
                table: "Edges");

            migrationBuilder.DropIndex(
                name: "IX_Edges_FromNodeId",
                table: "Edges");

            migrationBuilder.DropIndex(
                name: "IX_Edges_ToNodeId",
                table: "Edges");
        }
    }
}
