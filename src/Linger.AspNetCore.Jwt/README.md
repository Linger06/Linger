C# Helper Library

## Supported .NET versions

This library supports .NET applications that utilize .NET Framework 4.6.2+ or .NET Standard 2.0+.


Refresh tokens are credentials that can be used to acquire new access tokens. When access tokens expire, we can use refresh tokens to get a new access token from the authentication component. The lifetime of a refresh token is usually set much longer compared to the lifetime of an access token.

刷新令牌是可用于获取新访问令牌的凭据。当访问令牌过期时，我们可以使用刷新令牌从身份验证组件获取新的访问令牌。与访问令牌的生存期相比，刷新令牌的生存期通常设置得更长。

Refresh Token的过期时间应该要远超于Access Token
Access Token的过期时间通常设为几分钟，保存在Client端
Refresh Token的过期时间通常设为几天.保存在DB

## Token 使用步骤如下：

![Refresh Token Flow](refresh-token-flow.png "Refresh Token Flow")

1.First, the client authenticates with the authentication component by providing the credentials 

首先，客户端通过提供凭据来使用身份验证组件进行身份验证

2.Then, the authentication component issues the access token and the refresh token

然后，身份验证组件颁发访问令牌和刷新令牌

3.After that, the client requests the resource endpoints for a protected resource by providing the access token

之后，客户端通过提供访问令牌来请求受保护资源的资源终结点

4.The resource endpoint validates the access token and provides a protected resource

资源终结点验证访问令牌并提供受保护的资源

5.Steps 3 & 4 keep on repeating until the access token expires

第 3 步和第 4 步：继续重复，直到访问令牌过期

6.Once the access token expires, the client requests a new access token by providing the refresh token

访问令牌过期后，客户端通过提供刷新令牌来请求新的访问令牌

7.The authentication component issues a new access token and refresh token

身份验证组件颁发新的访问令牌和刷新令牌

8.Steps 3 through 7 keep on repeating until the refresh token expires

步骤 3 到 7 继续重复，直到刷新令牌过期

9.Once the refresh token expires, the client needs to authenticate with the authentication server once again and the flow repeats from step 1

刷新令牌过期后，客户端需要再次向身份验证服务器进行身份验证，并且从步骤 1 开始重复该流程



## The Need for Refresh Tokens
## 刷新令牌的需求
So, why do we need both access tokens and refresh tokens? Why don’t we just set a long expiration date, like a month or a year for the access tokens? Because, if we do that and someone manages to get hold of our access token they can use it for a long period, even if we change our password!

那么，为什么我们既需要访问令牌又需要刷新令牌呢？我们为什么不为访问令牌设置一个较长的到期日期，例如一个月或一年？因为，如果我们这样做并且有人设法获得我们的访问令牌，即使我们更改了密码，他们也可以长时间使用它！

The idea of refresh tokens is that we can make the access token short-lived so that, even if it is compromised, the attacker gets access only for a shorter period. With refresh token-based flow, the authentication server issues a one-time use refresh token along with the access token. The app stores the refresh token safely.

刷新令牌的想法是，我们可以使访问令牌的生存期很短，这样，即使它被破坏，攻击者也只能在较短的时间内获得访问权限。 使用基于刷新令牌的流，身份验证服务器会发出一次性使用的刷新令牌以及访问令牌。该应用程序安全地存储刷新令牌。

Every time the app sends a request to the server it sends the access token in the Authorization header and the server can identify the app using it. Once the access token expires, the server will send a token expired response. Once the app receives the token expired response, it sends the expired access token and the refresh token to obtain a new access token and refresh token. 

每次应用向服务器发送请求时，它都会在 Authorization 标头中发送访问令牌，服务器可以识别使用它的应用。一旦访问令牌过期，服务器将发送令牌过期的响应。应用收到令牌过期响应后，会发送过期的访问令牌和刷新令牌，以获取新的访问令牌和刷新令牌。 

If something goes wrong, the refresh token can be revoked which means that when the app tries to use it to get a new access token, that request will be rejected and the user will have to enter credentials once again and authenticate.

如果出现问题，刷新令牌可以被撤销，这意味着当应用尝试使用它来获取新的访问令牌时，该请求将被拒绝，用户必须再次输入凭据并进行身份验证。

Thus, refresh tokens help in a smooth authentication workflow without the need for users to submit their credentials frequently, and at the same time, without compromising the security of the app. 

因此，刷新令牌有助于顺利进行身份验证工作流，而无需用户频繁提交其凭据，同时又不会影响应用程序的安全性。