﻿using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Enums;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;
using FamilyHubs.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingTheVcsDashboard
{
    private readonly DashboardModel _pageModel;
    private readonly Mock<IReferralClientService> _mockReferralClientService;

    public WhenUsingTheVcsDashboard()
    {
        _mockReferralClientService = new Mock<IReferralClientService>();
        List<ReferralDto> list = new() { GetReferralDto() };
        PaginatedList<ReferralDto> pagelist = new PaginatedList<ReferralDto>(list, 1, 1, 1);
        _mockReferralClientService.Setup(x => x.GetRequestsForConnectionByOrganisationId(It.IsAny<string>(), It.IsAny<ReferralOrderBy>(), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pagelist);

        var displayName = "User name";
        var identity = new GenericIdentity(displayName);
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.Role, "Professional"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.OrganisationId, "1"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.AccountStatus, "active"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.FirstName, "Test"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.LastName, "User"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.LoginTime, DateTime.UtcNow.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Email, "Joe.Professional@email.com"));
        identity.AddClaim(new Claim(FamilyHubsClaimTypes.PhoneNumber, "012345678"));
        var principle = new ClaimsPrincipal(identity);
        // use default context with user
        var httpContext = new DefaultHttpContext()
        {
            User = principle
        };

        //need these as well for the page context
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
        // need page context for the page model
        var pageContext = new PageContext(actionContext)
        {
            ViewData = viewData
        };

        _pageModel = new DashboardModel(_mockReferralClientService.Object);
        _pageModel.PageContext = pageContext;
    }

    [Fact]
    public async Task ThenOnGetVcsDashboard()
    {
        //Act & Arrange
        await _pageModel.OnGet(ReferralOrderBy.NotSet.ToString(), false, 1);

        //Assert
        _pageModel.OrganisationId.Should().Be("1");
        _pageModel.SearchResults.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ThenOnPostVcsDashboard()
    {
        //Act & Arrange
        await _pageModel.OnPost("1", ReferralOrderBy.NotSet.ToString(), null, 1);

        //Assert
        _pageModel.SearchResults.TotalCount.Should().Be(1);
    }

    public static ReferralDto GetReferralDto()
    {
        return new ReferralDto
        {
            Id = 2,
            ReasonForSupport = "Reason For Support",
            EngageWithFamily = "Engage With Family",
            RecipientDto = new RecipientDto
            {
                Id = 2,
                Name = "Joe Blogs",
                Email = "JoeBlog@email.com",
                Telephone = "078123456",
                TextPhone = "078123456",
                AddressLine1 = "Address Line 1",
                AddressLine2 = "Address Line 2",
                TownOrCity = "Town or City",
                Country = "Country",
                PostCode = "B30 2TV"
            },
            ReferrerDto = new ReferrerDto
            {
                Id = 2,
                EmailAddress = "Bob.Referrer@email.com",
            },
            Status = new ReferralStatusDto
            {
                Name = "New",
                SortOrder = 0
            },
            ReferralServiceDto = new ReferralServiceDto
            {
                Id = 2,
                Name = "Service",
                Description = "Service Description",
                ReferralOrganisationDto = new ReferralOrganisationDto
                {
                    Id = 2,
                    Name = "Organisation",
                    Description = "Organisation Description",
                }
            }

        };
    }
}