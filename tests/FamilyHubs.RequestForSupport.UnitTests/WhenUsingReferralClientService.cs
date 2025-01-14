﻿using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ReferralService.Shared.Models;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Pages.Vcs;
using FamilyHubs.SharedKernel.Security;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Identity.Client;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FamilyHubs.RequestForSupport.UnitTests;

public class WhenUsingReferralClientService
{
    private readonly Mock<ICrypto> _cryptoMock;
    public WhenUsingReferralClientService()
    {
        _cryptoMock = new Mock<ICrypto>();  
    }

    [Fact]
    public async Task ThenGetRequestsByLaProfessional()
    {
        var accountId = "123";
        List<ReferralDto> listReferral = new List<ReferralDto>() { GetReferralDto() };
        PaginatedList<ReferralDto> expectedList = new PaginatedList<ReferralDto>(listReferral, 1, 1, 10);
        string jsonString = JsonSerializer.Serialize(expectedList);
        HttpClient httpClient = GetMockClient(jsonString);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer token");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        ReferralClientService referralClientService = new ReferralClientService(httpClient, _cryptoMock.Object);

        // Act
        var result = await referralClientService.GetRequestsByLaProfessional(accountId, null, null, 1, 10);

        // Assert
        result.Should().BeEquivalentTo(expectedList, options => options.Excluding(x => x.Items[0].ReasonForSupport).Excluding(x => x.Items[0].EngageWithFamily));
        _cryptoMock.Verify(c => c.DecryptData(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ThenGetRequestsForConnectionByOrganisationId()
    {
        var organisationId = "123";
        List<ReferralDto> listReferral = new List<ReferralDto>() { GetReferralDto() };
        PaginatedList<ReferralDto> expectedList = new PaginatedList<ReferralDto>(listReferral, 1, 1, 10);
        string jsonString = JsonSerializer.Serialize(expectedList);
        HttpClient httpClient = GetMockClient(jsonString);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer token");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        ReferralClientService referralClientService = new ReferralClientService(httpClient, _cryptoMock.Object);

        // Act
        var result = await referralClientService.GetRequestsForConnectionByOrganisationId(organisationId, null, null, 1, 10);

        // Assert
        result.Should().BeEquivalentTo(expectedList, options => options.Excluding(x => x.Items[0].ReasonForSupport).Excluding(x => x.Items[0].EngageWithFamily));
        _cryptoMock.Verify(c => c.DecryptData(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ThenGetReferralById()
    {
        var referralId = 1L;
        var expectedReferral = GetReferralDto();
        string jsonString = JsonSerializer.Serialize(expectedReferral);
        HttpClient httpClient = GetMockClient(jsonString);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer token");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        ReferralClientService referralClientService = new ReferralClientService(httpClient, _cryptoMock.Object);

        // Act
        var result = await referralClientService.GetReferralById(referralId);

        // Assert
        result.Should().BeEquivalentTo(expectedReferral, options => options.Excluding(x => x.ReasonForSupport).Excluding(x => x.EngageWithFamily));
        _cryptoMock.Verify(c => c.DecryptData(It.IsAny<string>()), Times.Exactly(2));
    }


    [Theory]
    [InlineData(ReferralStatus.Accepted)]
    [InlineData(ReferralStatus.Declined)]
    public async Task ThenUpdateReferralStatus(ReferralStatus expectedReferralStatus)
    {
        var referralId = 1L;
        string jsonString = JsonSerializer.Serialize(expectedReferralStatus.ToString());
        HttpClient httpClient = GetMockClient(jsonString);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer token");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        ReferralClientService referralClientService = new ReferralClientService(httpClient, _cryptoMock.Object);

        // Act
        var result = await referralClientService.UpdateReferralStatus(referralId, expectedReferralStatus);

        // Assert
        
        result.Replace("\"","").Should().Be(expectedReferralStatus.ToString());
      
    }

    private HttpClient GetMockClient(string content)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent(content),
                StatusCode = HttpStatusCode.OK
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        client.BaseAddress = new Uri("Https://Localhost");
        return client;
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
                County = "County",
                PostCode = "B30 2TV"
            },
            ReferralUserAccountDto = new UserAccountDto
            {
                Id = 2,
                EmailAddress = "Bob.Referrer@email.com",
                Name = "Bob Referrer",
                PhoneNumber = "1234567890",
                Team = "Team",
                UserAccountRoles = new List<UserAccountRoleDto>()
                    {
                        new UserAccountRoleDto
                        {
                            UserAccount = new UserAccountDto
                            {
                                EmailAddress = "Bob.Referrer@email.com",
                            },
                            Role = new RoleDto
                            {
                                Name = "VcsProfessional"
                            }
                        }
                    },
                ServiceUserAccounts = new List<UserAccountServiceDto>(),
                OrganisationUserAccounts = new List<UserAccountOrganisationDto>(),
            },
            Status = new ReferralStatusDto
            {
                Id = 1,
                Name = "New",
                SortOrder = 0
            },
            ReferralServiceDto = new ReferralServiceDto
            {
                Id = 2,
                Name = "Service",
                Description = "Service Description",
                Url = "www.service.com",
                OrganisationDto = new OrganisationDto
                {
                    Id = 2,
                    ReferralServiceId = 2,
                    Name = "Organisation",
                    Description = "Organisation Description",
                }
            }

        };
    }
}
