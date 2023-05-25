using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;

[Authorize]
public class VcsRequestDetailsPageModel : PageModel
{
    
    private readonly IReferralClientService _referralClientService;
    public ReferralDto Referral { get; set; } = default!;
    public ReferrerDtoEx ReferrerEx { get; set; } = default!;
    public bool ShowTeam { get; private set; }

    [BindProperty]
    public string? ReasonForRejection { get; set; }

    [BindProperty]
    public string? ServiceRequestResponse { get; set; }

    public VcsRequestDetailsPageModel(IReferralClientService referralClientService, IConfiguration configuration)
    {
        _referralClientService = referralClientService;
        ShowTeam = configuration.GetValue<bool>("ShowTeam");
    }

    public async Task OnGet(long referralId)
    {
        Referral = await _referralClientService.GetReferralById(referralId);
        ReferrerEx = ReferrerDtoEx.CreateReferrerDtoEx(Referral.ReferrerDto, ShowTeam);
    }

    public async Task OnPost(long referralId)
    {
        var referral = await _referralClientService.GetReferralById(referralId);

        List<ReferralStatusDto> statuses = await _referralClientService.GetReferralStatuses();

        if (ServiceRequestResponse == "Accepted")
        {
            ReferralStatusDto referralStatusDto = statuses.SingleOrDefault(x => x.Name == "Accepted") ?? new ReferralStatusDto { Name = "Unknown" };
            referral.Status = referralStatusDto;
            await _referralClientService.UpdateReferral(referral);
        }
        else
        {
            ReferralStatusDto referralStatusDto = statuses.SingleOrDefault(x => x.Name == "Declined") ?? new ReferralStatusDto { Name = "Unknown" };
            referral.Status = referralStatusDto;
            referral.ReasonForDecliningSupport = ReasonForRejection;
            await _referralClientService.UpdateReferral(referral);
        }
    }
}
