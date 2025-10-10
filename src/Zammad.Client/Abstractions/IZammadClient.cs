namespace Zammad.Client.Abstractions;

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
