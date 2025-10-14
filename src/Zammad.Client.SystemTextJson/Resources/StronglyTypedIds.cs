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
public partial struct ObjectLookupId;

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
public partial struct PriorityId;

[StronglyTypedId]
public partial struct StateId;

[StronglyTypedId]
public partial struct StateTypeId;

[StronglyTypedId]
public partial struct EmailAddressId;

[StronglyTypedId]
public partial struct SignatureId;
