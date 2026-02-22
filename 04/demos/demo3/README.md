# End-user Authorization

In this demo we'll add security for application users, requiring end-user authentication with JWT and enforcing access control based on the JWT calims.

## References

- [Istio authentication architecture](https://istio.io/latest/docs/concepts/security/#authentication-architecture)

- [AuthorizationPolicy conditions](https://istio.io/latest/docs/reference/config/security/conditions/)

## Pre-reqs

Follow the steps from [demo 1](../demo1/README.md) to deploy a working instance of bookinfo with PeerAuthentication applied.

```
kubectl config set-context --current --namespace=bookinfo
```

> Browse to http://localhost/productpage 

## Require JWT authentication

Apply the JWT authentication policy for the product page:

- [productpage-authn-jwt.yaml](productpage-authn-jwt.yaml)

```
kubectl apply -f demo3/productpage-authn-jwt.yaml
```

> Browse to http://localhost/productpage -> `RBAC: access denied`


``` 
curl -v http://localhost/productpage
```

> 403

Try with any old JWT (from https://jwt.io):

```
curl -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c' -v http://localhost/productpage
```

> 401, JWT issuer not configured

Use a valid JWT (from the [Istio sample JWT tools](https://github.com/istio/istio/blob/master/security/tools/jwt/samples/README.md)):

```
curl -H 'Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IkRIRmJwb0lVcXJZOHQyenBBMnFYZkNtcjVWTzVaRXI0UnpIVV8tZW52dlEiLCJ0eXAiOiJKV1QifQ.eyJleHAiOjQ2ODU5ODk3MDAsImZvbyI6ImJhciIsImlhdCI6MTUzMjM4OTcwMCwiaXNzIjoidGVzdGluZ0BzZWN1cmUuaXN0aW8uaW8iLCJzdWIiOiJ0ZXN0aW5nQHNlY3VyZS5pc3Rpby5pbyJ9.CfNnxWP2tcnR9q0vxyxweaF3ovQYHYZl82hAUsn21bwQd9zP7c-LS9qd_vpdLG4Tn1A15NxfCjp5f7QNBUo-KC9PJqYpgGbaXhaGx7bEdFWjcwv3nZzvc7M__ZpaCERdwU7igUmJqYGBYQ51vr2njU9ZimyKkfDe3axcyiBZde7G6dabliUosJvvKOPcKIWPccCgefSj_GNfwIip3-SsFdlR7BtbVUcqR-yv-XOxJ3Uc1MI0tz3uMiiZcyPV7sNCU4KRnemRIMHVOfuvHsU60_GhGbiSFzgPTAa9WTltbnarTbxudb_YEOx12JiwYToeX0DCPb43W1tzIBxgm8NxUg' -v http://localhost/productpage
```

> `200`

## Decode the JWT

The JWT is a base64 encoded string. Read the claims - browse to 
 https://jwt.io and paste contents of [demo.jwt](demo.jwt)

- Issuer: `testing@secure.istio.io`
- Subject: `testing@secure.istio.io`
- Custom: `foo=bar`

## Allow access by claims

Apply an authorization policy which allows access by issuer:

- [productpage-authz-allow-issuer.yaml](productpage-authz-allow-issuer.yaml)

```
kubectl apply -f demo3/productpage-authz-allow-issuer.yaml
```

> Repeat curl request -> `200`

Apply an authorization policy which allows access by issuer & subject:

- [productpage-authz-allow-subject.yaml](productpage-authz-allow-subject.yaml)

```
kubectl apply -f demo3/productpage-authz-allow-subject.yaml
```
> Repeat curl request -> `403`

Apply an authorization policy which allows access by issuer and claim:

- [productpage-authz-allow-claim.yaml](productpage-authz-allow-claim.yaml)

```
kubectl apply -f productpage-authz-allow-claim.yaml
```
> Repeat curl request -> `200`

## Integration with third-party auth

The same RequestAuthentication resource, with the identity provider's details plugged in, e.g. for Azure AD:

- [productpage-auth-azure.yaml](productpage-auth-azure.yaml)