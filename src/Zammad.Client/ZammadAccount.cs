﻿using System;
using Zammad.Client.Core;

namespace Zammad.Client;

#nullable enable

/// <summary>
/// Represents a Zammad account with which clients can be created for Zammad resources.
/// </summary>
public class ZammadAccount
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZammadAccount"/> class.
    /// </summary>
    /// <param name="endpoint">The endpoint of the Zammad instance being used.</param>
    /// <param name="authentication">Specifies the authentication method who is used for authentication.</param>
    /// <param name="user">The user who is used for authentication.</param>
    /// <param name="password">The password who is used for authentication.</param>
    /// <param name="token">The token who is used for authentication.</param>
    public ZammadAccount(
        Uri endpoint,
        ZammadAuthentication authentication,
        string? user,
        string? password,
        string? token
    )
    {
        if (endpoint is null)
        {
            throw new ArgumentNullException(nameof(endpoint));
        }

        switch (authentication)
        {
            case ZammadAuthentication.Basic:
                if (string.IsNullOrEmpty(user))
                {
                    throw new ArgumentOutOfRangeException(nameof(user));
                }

                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentOutOfRangeException(nameof(password));
                }

                break;
            case ZammadAuthentication.Token:
                if (string.IsNullOrEmpty(token))
                {
                    throw new ArgumentOutOfRangeException(nameof(token));
                }

                break;
            default:
                throw new NotSupportedException($"Authentication \"{authentication}\" is not supported.");
        }

        Endpoint = endpoint;
        Authentication = authentication;
        Token = token;
        User = user;
        Password = password;
    }

    public Uri Endpoint { get; }
    public ZammadAuthentication Authentication { get; }
    public string? User { get; }
    public string? Password { get; }
    public string? Token { get; }
    public string? OnBehalfOf { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZammadAccount"/> class that uses the basic authentication method.
    /// </summary>
    /// <param name="endpoint">The endpoint of the Zammad instance being used.</param>
    /// <param name="user">The user who is used for authentication.</param>
    /// <param name="password">The password who is used for authentication.</param>
    /// <returns>A new instance of the <see cref="ZammadAccount"/> class that uses the basic authentication method.</returns>
    public static ZammadAccount CreateBasicAccount(string endpoint, string user, string password) =>
        CreateBasicAccount(new Uri(endpoint), user, password);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZammadAccount"/> class that uses the token authentication method.
    /// </summary>
    /// <param name="endpoint">The endpoint of the Zammad instance being used.</param>
    /// <param name="user">The user who is used for authentication.</param>
    /// <param name="password">The password who is used for authentication.</param>
    /// <returns>A new instance of the <see cref="ZammadAccount"/> class that uses the basic authentication method.</returns>
    public static ZammadAccount CreateBasicAccount(Uri endpoint, string user, string password) =>
        new ZammadAccount(endpoint, ZammadAuthentication.Basic, user, password, null);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZammadAccount"/> class that uses the token authentication method.
    /// </summary>
    /// <param name="endpoint">The endpoint of the Zammad instance being used.</param>
    /// <param name="token">The token who is used for authentication.</param>
    /// <returns>A new instance of the <see cref="ZammadAccount"/> class that uses the token authentication method.</returns>
    public static ZammadAccount CreateTokenAccount(string endpoint, string token) =>
        CreateTokenAccount(new Uri(endpoint), token);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZammadAccount"/> class that uses the token authentication method.
    /// </summary>
    /// <param name="endpoint">The endpoint of the Zammad instance being used.</param>
    /// <param name="token">The token who is used for authentication.</param>
    /// <returns>A new instance of the <see cref="ZammadAccount"/> class that uses the token authentication method.</returns>
    public static ZammadAccount CreateTokenAccount(Uri endpoint, string token) =>
        new ZammadAccount(endpoint, ZammadAuthentication.Token, null, null, token);

    /// <summary>
    /// Instructs the <see cref="ZammadAccount"/> to set the X-On-Behalf-Of header on requests.
    /// </summary>
    /// <param name="user">One of the three possible values: user id, user login, user email</param>
    /// <returns>A instance of the <see cref="ZammadAccount"/> class that uses the X-On-Behalf-Of header.</returns>
    public ZammadAccount UseOnBehalfOf(string user)
    {
        OnBehalfOf = user;
        return this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupClient"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="GroupClient"/> class.</returns>
    public GroupClient CreateGroupClient() => new GroupClient(this);

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectClient"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="ObjectClient"/> class.</returns>
    public ObjectClient CreateObjectClient() => new ObjectClient(this);

    /// <summary>
    /// Initializes a new instance of the <see cref="OnlineNotificationClient"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="OnlineNotificationClient"/> class.</returns>
    public OnlineNotificationClient CreateOnlineNotificationClient() => new OnlineNotificationClient(this);

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationClient"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="OrganizationClient"/> class.</returns>
    public OrganizationClient CreateOrganizationClient() => new OrganizationClient(this);

    /// <summary>
    /// Initializes a new instance of the <see cref="TagClient"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="TagClient"/> class.</returns>
    public TagClient CreateTagClient() => new TagClient(this);

    /// <summary>
    /// Initializes a new instance of the <see cref="TicketClient"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="TicketClient"/> class.</returns>
    public TicketClient CreateTicketClient() => new TicketClient(this);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserClient"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="UserClient"/> class.</returns>
    public UserClient CreateUserClient() => new UserClient(this);
}
