using StronglyTypedIds;

[assembly: StronglyTypedIdDefaults(Template.Int)]

namespace Zammad.Client.Resources;

[StronglyTypedId]
public partial struct UserId;

[StronglyTypedId]
public partial struct GroupId;

[StronglyTypedId]
public partial struct ObjectId;

[StronglyTypedId]
public partial struct TargetObjectId;

public static class TargetObjectIdExtensions
{
    public static TargetObjectId ToTargetObjectId(this TicketId id) => new(id.Value);

    public static TargetObjectId ToTargetObjectId(this UserId id) => new(id.Value);

    public static TargetObjectId ToTargetObjectId(this OrganizationId id) => new(id.Value);

    public static TargetObjectId ToTargetObjectId(this GroupId id) => new(id.Value);

    public static TargetObjectId ToTargetObjectId(this ArticleId id) => new(id.Value);
}

[StronglyTypedId]
public partial struct ObjectLookupId;

[StronglyTypedId]
public partial struct TypeLookupId;

[StronglyTypedId]
public partial struct NotificationId;

[StronglyTypedId]
public partial struct OrganizationId;

[StronglyTypedId]
public partial struct TagId;

[StronglyTypedId]
public partial struct TicketId;

[StronglyTypedId]
public partial struct ArticleId;

[StronglyTypedId]
public partial struct ArticleTypeId;

/// <summary>
/// ID of a <c>Ticket::Article::Sender</c> (by default 1 = Agent, 2 = Customer, 3 = System), not a user.
/// </summary>
[StronglyTypedId]
public partial struct ArticleSenderId;

[StronglyTypedId]
public partial struct AttachmentId;

[StronglyTypedId]
public partial struct StoreFileId;

[StronglyTypedId]
public partial struct PriorityId;

[StronglyTypedId]
public partial struct StateId;

[StronglyTypedId]
public partial struct StateTypeId;

[StronglyTypedId]
public partial struct EmailAddressId;

[StronglyTypedId]
public partial struct SignatureId;

[StronglyTypedId]
public partial struct ChannelId;

[StronglyTypedId]
public partial struct ChecklistId;

[StronglyTypedId]
public partial struct ChecklistItemId;

[StronglyTypedId]
public partial struct ChecklistTemplateId;

[StronglyTypedId]
public partial struct ChecklistTemplateItemId;

[StronglyTypedId]
public partial struct MentionId;

[StronglyTypedId]
public partial struct TimeAccountingId;

[StronglyTypedId]
public partial struct TimeAccountingTypeId;

[StronglyTypedId]
public partial struct AIStoredResultId;

[StronglyTypedId]
public partial struct RoleId;

[StronglyTypedId]
public partial struct TwoFactorPreferenceId;

[StronglyTypedId]
public partial struct AuthorizationId;

[StronglyTypedId]
public partial struct OverviewSortingId;

[StronglyTypedId]
public partial struct DailyEventLockId;

[StronglyTypedId]
public partial struct DataPrivacyTaskId;

[StronglyTypedId]
public partial struct SettingId;

[StronglyTypedId]
public partial struct MacroId;

[StronglyTypedId]
public partial struct OverviewId;

[StronglyTypedId]
public partial struct TemplateId;

[StronglyTypedId]
public partial struct TextModuleId;

[StronglyTypedId]
public partial struct TriggerId;

/// <summary>
/// ID of a scheduler (<c>Job</c> in Zammad's API).
/// </summary>
[StronglyTypedId]
public partial struct JobId;

[StronglyTypedId]
public partial struct WebhookId;

[StronglyTypedId]
public partial struct CoreWorkflowId;

[StronglyTypedId]
public partial struct SlaId;

[StronglyTypedId]
public partial struct CalendarId;

[StronglyTypedId]
public partial struct PostmasterFilterId;
