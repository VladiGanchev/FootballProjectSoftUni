using AutoMapper;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Tests.Mocks;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FootballProjectSoftUni.Infrastructure.Data.Models;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class UnitTestsBase
    {
        protected ApplicationDbContext _data;

        [SetUp]

        public void SetUpBase()
        {
            _data = DatabaseMock.Instance;
            SeedDatabase();
        }

        public City City { get; private set; }
        private void SeedDatabase()
        {
            City = new City()
            {
                Id = 1,
                Name = "Благоевград"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 2,
                Name = "Бургас"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 3,
                Name = "Варна"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 4,
                Name = "Велико Търново"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 5,
                Name = "Видин"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 6,
                Name = "Враца"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 7,
                Name = "Габрово"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 8,
                Name = "Добрич"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 9,
                Name = "Кърджали"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 10,
                Name = "Кюстендил"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 11,
                Name = "Ловеч"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 12,
                Name = "Монтана"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 13,
                Name = "Пазарджик"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 14,
                Name = "Плевен"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 15,
                Name = "Перник"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 16,
                Name = "Пловдив"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 17,
                Name = "Разград"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 18,
                Name = "Русе"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 19,
                Name = "Силистра"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 20,
                Name = "Сливен"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 21,
                Name = "Смолян"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 22,
                Name = "София"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 23,
                Name = "Стара Загора"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 24,
                Name = "Търговище"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 25,
                Name = "Хасково"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 26,
                Name = "Шумен"
            };
            _data.Cities.Add(City);
            City = new City()
            {
                Id = 27,
                Name = "Ямбол"
            };
            _data.Cities.Add(City);


            _data.SaveChanges();
        }

        [TearDown]
        public void TearDownBase()
    => _data.Dispose();

        //[OneTimeTearDown]

        //public void TearDownBase() => _data.Dispose();
    }
}
