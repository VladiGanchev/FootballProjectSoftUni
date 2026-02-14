using FootballProjectSoftUni.Core.Contracts.Partner;
using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Models.Partner;
using FootballProjectSoftUni.Core.Services.Partner;
using FootballProjectSoftUni.Core.Services.Profile;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class PartnerServiceTests : UnitTestsBase
    {
        private IPartnerService partnerService;

        [SetUp]
        public void SetUp() => partnerService = new PartnerService(_data);

        [Test]
        public async Task AddPartnerAsync_ShouldAddPartner_WhenModelIsValid()
        {
            var model = new PartnerViewModel
            {
                Name = "Partner 1",
                ImageUrl = "https://example.com/p1.png"
            };

            var before = await _data.Partners.CountAsync();

            await partnerService.AddPartnerAsync(model);

            var after = await _data.Partners.CountAsync();
            Assert.That(after, Is.EqualTo(before + 1));

            var added = await _data.Partners
                .OrderByDescending(p => p.Id)
                .FirstAsync();

            Assert.That(added.Name, Is.EqualTo("Partner 1"));
            Assert.That(added.ImageUrl, Is.EqualTo("https://example.com/p1.png"));
        }

        [Test]
        public async Task AllPartnersAsync_ShouldReturnEmpty_WhenNoPartners()
        {
            var result = await partnerService.AllPartnersAsync();

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task AllPartnersAsync_ShouldReturnAllPartners()
        {
            await _data.Partners.AddRangeAsync(
                new Partner { Name = "P1", ImageUrl = "img1" },
                new Partner { Name = "P2", ImageUrl = "img2" }
            );
            await _data.SaveChangesAsync();

            var result = (await partnerService.AllPartnersAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(p => p.Name == "P1" && p.ImageUrl == "img1"), Is.True);
            Assert.That(result.Any(p => p.Name == "P2" && p.ImageUrl == "img2"), Is.True);
        }

        [Test]
        public async Task DeletePartnerAsync_ShouldReturnFalse_WhenPartnerNotFound()
        {
            var result = await partnerService.DeletePartnerAsync(99999);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeletePartnerAsync_ShouldDeletePartner_WhenExists()
        {
            var partner = new Partner { Name = "DeleteMe", ImageUrl = "img" };
            await _data.Partners.AddAsync(partner);
            await _data.SaveChangesAsync();

            var result = await partnerService.DeletePartnerAsync(partner.Id);

            Assert.IsTrue(result);

            var exists = await _data.Partners.FindAsync(partner.Id);
            Assert.IsNull(exists);
        }

        [Test]
        public async Task FindPartnerAsync_ShouldReturnNull_WhenPartnerNotFound()
        {
            var result = await partnerService.FindPartnerAsync(new PartnerViewModel(), 99999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task FindPartnerAsync_ShouldReturnMappedViewModel_WhenPartnerExists()
        {
            var partner = new Partner
            {
                Name = "FindMe",
                ImageUrl = "img"
            };

            await _data.Partners.AddAsync(partner);
            await _data.SaveChangesAsync();

            var result = await partnerService.FindPartnerAsync(new PartnerViewModel(), partner.Id);

            Assert.IsNotNull(result);
            Assert.That(result!.Id, Is.EqualTo(partner.Id));
            Assert.That(result.Name, Is.EqualTo("FindMe"));
        }

    }
}
