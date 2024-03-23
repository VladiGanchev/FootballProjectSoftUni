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
                 Name = "Благоевград"
             },
             new City()
             {
                 Id = 2,
                 Name = "Бургас"
             },
             new City()
             {
                 Id = 3,
                 Name = "Варна"
             },
             new City()
             {
                 Id = 4,
                 Name = "Велико Търново"
             },
             new City()
             {
                 Id = 5,
                 Name = "Видин"
             },
             new City()
             {
                 Id = 6,
                 Name = "Враца"
             },
             new City()
             {
                 Id = 7,
                 Name = "Габрово"
             },
             new City()
             {
                 Id = 8,
                 Name = "Добрич"
             },
             new City()
             {
                 Id = 9,
                 Name = "Кърджали"
             },
             new City()
             {
                 Id = 10,
                 Name = "Кюстендил"
             },
             new City()
             {
                 Id = 11,
                 Name = "Ловеч"
             },
             new City()
             {
                 Id = 12,
                 Name = "Монтана"
             },
             new City()
             {
                 Id = 13,
                 Name = "Пазарджик"
             },
             new City()
             {
                 Id = 14,
                 Name = "Плевен"
             },
             new City()
             {
                 Id = 15,
                 Name = "Перник"
             },
             new City()
             {
                 Id = 16,
                 Name = "Пловдив"
             },
             new City()
             {
                 Id = 17,
                 Name = "Разград"
             },
             new City()
             {
                 Id = 18,
                 Name = "Русе"
             },
             new City()
             {
                 Id = 19,
                 Name = "Силистра"
             },
             new City()
             {
                 Id = 20,
                 Name = "Сливен"
             },
             new City()
             {
                 Id = 21,
                 Name = "Смолян"
             },
             new City()
             {
                 Id = 22,
                 Name = "София"
             },
             new City()
             {
                 Id = 23,
                 Name = "Стара Загора"
             },
             new City()
             {
                 Id = 24,
                 Name = "Търговище"
             },
             new City()
             {
                 Id = 25,
                 Name = "Хасково"
             },
             new City()
             {
                 Id = 26,
                 Name = "Шумен"
             },
             new City()
             {
                 Id = 27,
                 Name = "Ямбол"
             });
        }
    }
}
