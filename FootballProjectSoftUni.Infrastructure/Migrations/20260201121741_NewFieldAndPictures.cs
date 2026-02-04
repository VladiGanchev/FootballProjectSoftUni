using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class NewFieldAndPictures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Tournaments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f6f8482b-3c67-475f-9c27-eaa0923d6768", "AQAAAAEAACcQAAAAEBRGz04eYo5UPlF3q63AnnpyJLGsOrQ3AedfRl4eJVfRrnFaDcYSPuNxbl9VxGB+YQ==", "8a9e3d66-1a74-4f46-b3bf-6db333cc8e2c" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "https://e-tourguide.eu/wp-content/uploads/2023/09/BL-d-scaled.jpeg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "https://www.mywanderlust.pl/wp-content/uploads/2021/07/things-to-do-in-burgas-bulgaria-138.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "https://dynamic-media-cdn.tripadvisor.com/media/photo-o/2b/e3/5b/5d/theotokos-cathedral.jpg?w=900&h=500&s=1");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImageUrl",
                value: "https://freshholiday.bg/img/NOVINI/BIG_230127093507veliko-tarnovo-podmeni-ulichnoto-osvetlenie-v-chetiri-kvartala_1706007449223.jpg.webp");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImageUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSxzE-q0_id6nvgsBF_h2zLdZ8ddlrPOC5mKw&s");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImageUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTvq8CUaPtVAQ8J4U4nNnfccmJ-KsHOqX5irg&s");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImageUrl",
                value: "https://www.gabrovodaily.info/wp-content/uploads/2016/06/Obshtina-Gabrovo.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImageUrl",
                value: "https://bgtourism.bg/wp-content/uploads/2023/07/93DC82F8-DE23-4016-854D-02C8B622E7A7.jpeg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImageUrl",
                value: "https://visitkardzhali.com/wp-content/uploads/2024/11/kj1-900x500.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImageUrl",
                value: "https://visit-kyustendil.eu/images/kustendil-ploshtada.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 11,
                column: "ImageUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcROnOulepJWi_vxDqO3Ma5HMQz9_Xv35UcDPg&s");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 12,
                column: "ImageUrl",
                value: "https://sportenkalendar.bg/media/cache/resolve/event_medium_thumbs/uploads/pages/zheravitsa-63affff2b6dd9672914279.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 13,
                column: "ImageUrl",
                value: "https://cache1.24chasa.bg/Images/Cache/949/IMAGE_16373949_40_0.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 14,
                column: "ImageUrl",
                value: "https://www.pleven.bg/uploads/posts/19.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 15,
                column: "ImageUrl",
                value: "https://img.haskovo.net//images/news_images/2023/02/01/orig-80130789772169881.jpg?v=1675242496");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 16,
                column: "ImageUrl",
                value: "https://img.capital.bg/shimg/zx1200y675_3964872.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 17,
                column: "ImageUrl",
                value: "https://www.eufunds.bg/sites/default/files/uploads/oic/doc-icons/2020-04/Eufunds.bg%20-%20WiFi4EU%20%D0%A0%D0%B0%D0%B7%D0%B3%D1%80%D0%B0%D0%B4.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 18,
                column: "ImageUrl",
                value: "https://faiton.bg/wp-content/uploads/2024/08/ruse-1-780x470.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 19,
                column: "ImageUrl",
                value: "https://www.portal-silistra.eu/images/guide/2d698621d7c532d857158ecc331fb462.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 20,
                column: "ImageUrl",
                value: "https://new.sliven.net/res/news/316912/65920fe5e324526584fbcaa7643ff457.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 21,
                column: "ImageUrl",
                value: "https://visitsmolyan.bg/wp-content/uploads/2020/10/%D0%B3%D1%80.-%D0%A1%D0%BC%D0%BE%D0%BB%D1%8F%D0%BD-%D0%BA%D0%B2.-%D0%A3%D1%81%D1%82%D0%BE%D0%B2%D0%BE-%D0%A3%D0%B8%D0%BA%D0%B8%D0%BF%D0%B5%D0%B4%D0%B8%D1%8F.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 22,
                column: "ImageUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR3NOq-4JtUcn_sFQQ9ztnTfVqfzfkNTSfG5A&s");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 23,
                column: "ImageUrl",
                value: "https://www.luximmo.bg/town-images/big/56_11.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 24,
                column: "ImageUrl",
                value: "https://www.luximmo.bg/town-images/big/50_1.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 25,
                column: "ImageUrl",
                value: "https://visithaskovo.com/wp-content/uploads/2019/09/kulacentre.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 26,
                column: "ImageUrl",
                value: "https://img.capital.bg/shimg/zx1200y675_4220352.jpg");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 27,
                column: "ImageUrl",
                value: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS0i1VLsI6GP_296dBpbHfcamtBo1OUvhxLsg&s");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Tournaments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bb651cdd-eccd-4570-a8e4-f14d90a0be61", "AQAAAAEAACcQAAAAEAfNA3nUR9eS3k5yX0uYdnl3BFu7iJnPfsL8O37lLAnfdmT0lbfE4AxuT2JNGTsXog==", "4362b4e7-451d-43f8-b1e5-f265cc6c3432" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 7,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 8,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 9,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 10,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 11,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 12,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 13,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 14,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 15,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 16,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 17,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 18,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 19,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 20,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 21,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 22,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 23,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 24,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 25,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 26,
                column: "ImageUrl",
                value: "");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 27,
                column: "ImageUrl",
                value: "");
        }
    }
}
