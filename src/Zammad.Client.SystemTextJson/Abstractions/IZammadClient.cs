namespace Zammad.Client.Abstractions;

#nullable enable
public interface IZammadClient
    : IGroupService,
        IObjectService,
        IOnlineNotificationService,
        IOrganizationService,
        ITagService,
        ITicketArticleService,
        ITicketPriorityService,
        ITicketService,
        ITicketStateService,
        IUserService;
