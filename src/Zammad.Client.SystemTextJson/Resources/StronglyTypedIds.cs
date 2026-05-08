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
public partial struct ChecklistId;

[StronglyTypedId]
public partial struct TimeAccountingId;

[StronglyTypedId]
public partial struct TimeAccountingTypeId;
