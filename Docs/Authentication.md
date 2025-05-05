﻿# Authentication
If your application need access to protected species, you need to login with your Artdatabanken (Artportalen) user account to get an access token.
 SOS supports OAuth 2.0.

## Subscription key
To access any of the API:s an subscription key must be added to your request. Key are received by signing-in and requesting a subscription to any of the SLU Swedish Species Information Centre´s products. The keys are than found in your profile in the developer portal.

A key must always be added to any requests to the API:s. The key should be added as a HTTP request header:

`Ocp-Apim-Subscription-Key: {key}`

## Authorization
Requests to the SLU Swedish Species Information Centre´s Web API:s may also require authorization, ie the end user must have granted permission for an application to access/change data on behalf of the user. The application requests the permission by redirecting the user to the Login Service web application, which returns an authorization token to the application. To prove that the user was granted permission for the application, an HTTP header, with a valid authorization token, should be sent in each request to the API by the application.

`Authorization: Bearer {authentication token}`

### OAuth 2 and OpenID Connect
The Web API uses the OAuth 2 and OpenID Connect standards during authorization.

As the first step towards authorization, you will need to receive your unique client ID and client secret key from SLU Swedish Species Information Centre to use in the authorization flows. You will also need to specify the return URL where the token should be delivered on login. Contact support to exchange authorization details.

### Supported Authorization Flows
- Authorization Code Grant Flow with PKCE ([RFC-7636](https://datatracker.ietf.org/doc/html/rfc7636))

## URL:s

| Name 	| Value 	|
|-	|-	|
| Scope 	| SOS.Observations.Protected  	|
| OAuth 2.0 Server 	| https://useradmin-auth.slu.se 	|
| Auth URL 	| https://useradmin-auth.slu.se/connect/authorize 	|
| Access token URL 	| https://useradmin-auth.slu.se/connect/token 	|


![Login page](Images/idp-login.png "Login page")