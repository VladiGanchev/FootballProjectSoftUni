using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Configuration
{
    internal class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder
             .HasData(new City()
             {
                 Id = 1,
                 Name = "Благоевград",
                 ImageUrl= "https://e-tourguide.eu/wp-content/uploads/2023/09/BL-d-scaled.jpeg"
             },
             new City()
             {
                 Id = 2,
                 Name = "Бургас",
                 ImageUrl = "https://www.mywanderlust.pl/wp-content/uploads/2021/07/things-to-do-in-burgas-bulgaria-138.jpg"
             },
             new City()
             {
                 Id = 3,
                 Name = "Варна",
                 ImageUrl = "https://dynamic-media-cdn.tripadvisor.com/media/photo-o/2b/e3/5b/5d/theotokos-cathedral.jpg?w=900&h=500&s=1"
             },
             new City()
             {
                 Id = 4,
                 Name = "Велико Търново",
                 ImageUrl = "https://freshholiday.bg/img/NOVINI/BIG_230127093507veliko-tarnovo-podmeni-ulichnoto-osvetlenie-v-chetiri-kvartala_1706007449223.jpg.webp"
             },
             new City()
             {
                 Id = 5,
                 Name = "Видин",
                 ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSxzE-q0_id6nvgsBF_h2zLdZ8ddlrPOC5mKw&s"
             },
             new City()
             {
                 Id = 6,
                 Name = "Враца",
                 ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTvq8CUaPtVAQ8J4U4nNnfccmJ-KsHOqX5irg&s"
             },
             new City()
             {
                 Id = 7,
                 Name = "Габрово",
                 ImageUrl = "https://www.gabrovodaily.info/wp-content/uploads/2016/06/Obshtina-Gabrovo.jpg"
             },
             new City()
             {
                 Id = 8,
                 Name = "Добрич",
                 ImageUrl = "https://bgtourism.bg/wp-content/uploads/2023/07/93DC82F8-DE23-4016-854D-02C8B622E7A7.jpeg"
             },
             new City()
             {
                 Id = 9,
                 Name = "Кърджали",
                 ImageUrl = "https://visitkardzhali.com/wp-content/uploads/2024/11/kj1-900x500.jpg"
             },
             new City()
             {
                 Id = 10,
                 Name = "Кюстендил",
                 ImageUrl = "https://visit-kyustendil.eu/images/kustendil-ploshtada.jpg"
             },
             new City()
             {
                 Id = 11,
                 Name = "Ловеч",
                 ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcROnOulepJWi_vxDqO3Ma5HMQz9_Xv35UcDPg&s"
             },
             new City()
             {
                 Id = 12,
                 Name = "Монтана",
                 ImageUrl = "https://sportenkalendar.bg/media/cache/resolve/event_medium_thumbs/uploads/pages/zheravitsa-63affff2b6dd9672914279.jpg"
             },
             new City()
             {
                 Id = 13,
                 Name = "Пазарджик",
                 ImageUrl = "https://cache1.24chasa.bg/Images/Cache/949/IMAGE_16373949_40_0.jpg"
             },
             new City()
             {
                 Id = 14,
                 Name = "Плевен",
                 ImageUrl = "https://www.pleven.bg/uploads/posts/19.jpg"
             },
             new City()
             {
                 Id = 15,
                 Name = "Перник",
                 ImageUrl = "https://img.haskovo.net//images/news_images/2023/02/01/orig-80130789772169881.jpg?v=1675242496"
             },
             new City()
             {
                 Id = 16,
                 Name = "Пловдив",
                 ImageUrl = "https://img.capital.bg/shimg/zx1200y675_3964872.jpg"
             },
             new City()
             {
                 Id = 17,
                 Name = "Разград",
                 ImageUrl = "https://www.eufunds.bg/sites/default/files/uploads/oic/doc-icons/2020-04/Eufunds.bg%20-%20WiFi4EU%20%D0%A0%D0%B0%D0%B7%D0%B3%D1%80%D0%B0%D0%B4.jpg"
             },
             new City()
             {
                 Id = 18,
                 Name = "Русе",
                 ImageUrl = "https://faiton.bg/wp-content/uploads/2024/08/ruse-1-780x470.jpg"
             },
             new City()
             {
                 Id = 19,
                 Name = "Силистра",
                 ImageUrl = "https://www.portal-silistra.eu/images/guide/2d698621d7c532d857158ecc331fb462.jpg"
             },
             new City()
             {
                 Id = 20,
                 Name = "Сливен",
                 ImageUrl = "https://new.sliven.net/res/news/316912/65920fe5e324526584fbcaa7643ff457.jpg"
             },
             new City()
             {
                 Id = 21,
                 Name = "Смолян",
                 ImageUrl = "https://visitsmolyan.bg/wp-content/uploads/2020/10/%D0%B3%D1%80.-%D0%A1%D0%BC%D0%BE%D0%BB%D1%8F%D0%BD-%D0%BA%D0%B2.-%D0%A3%D1%81%D1%82%D0%BE%D0%B2%D0%BE-%D0%A3%D0%B8%D0%BA%D0%B8%D0%BF%D0%B5%D0%B4%D0%B8%D1%8F.jpg"
             },
             new City()
             {
                 Id = 22,
                 Name = "София",
                 ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR3NOq-4JtUcn_sFQQ9ztnTfVqfzfkNTSfG5A&s"
             },
             new City()
             {
                 Id = 23,
                 Name = "Стара Загора",
                 ImageUrl = "https://www.luximmo.bg/town-images/big/56_11.jpg"
             },
             new City()
             {
                 Id = 24,
                 Name = "Търговище",
                 ImageUrl = "https://www.luximmo.bg/town-images/big/50_1.jpg"
             },
             new City()
             {
                 Id = 25,
                 Name = "Хасково",
                 ImageUrl = "https://visithaskovo.com/wp-content/uploads/2019/09/kulacentre.jpg"
             },
             new City()
             {
                 Id = 26,
                 Name = "Шумен",
                 ImageUrl = "https://img.capital.bg/shimg/zx1200y675_4220352.jpg"
             },
             new City()
             {
                 Id = 27,
                 Name = "Ямбол",
                 ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS0i1VLsI6GP_296dBpbHfcamtBo1OUvhxLsg&s"
             });
        }
    }
}
