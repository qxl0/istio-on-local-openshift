# Service Authorization

In this demo we apply authorization betweeen back-end components, enforcing access control for services based on the caller's mTLS identity.

## References

- [Istio authorization architecture](https://istio.io/latest/docs/concepts/security/#authorization-architecture)

- [AuthorizationPolicy spec](https://istio.io/latest/docs/reference/config/security/authorization-policy/)

## Pre-reqs

Follow the steps from [demo 1](../demo1/README.md) to deploy a working instance of bookinfo with PeerAuthentication applied.

```
kubectl config set-context --current --namespace=bookinfo
```

## Restrict access to reviews

Currently the app has strict mTLS but no authorization:

```
kubectl describe peerauthentication

kubectl get authorizationpolicy
```

And it works:

> Check http://localhost/productpage

Apply a deny-all authorization policy for the reviews service, which will block all requests:

- [reviews-deny-all.yaml](reviews-deny-all.yaml)

```
kubectl apply -f demo2/reviews-deny-all.yaml
```

> Check http://localhost/productpage

## Allow access from product page

Istio uses the identity of the Service Account in the Pod as the secure name in the TLS certificate:

```
kubectl get serviceaccount

kubectl get po -l app=productpage -o json | jq '.items[0].spec.serviceAccountName'
```

Apply the updated authorization policy to allow client access from the product page:

- [reviews-allow-productpage.yaml](reviews-allow-productpage.yaml)

```
kubectl apply -f demo2/reviews-allow-productpage.yaml
```

> And check http://localhost/productpage

## Try from unauthorized service

Deploy the legacy container in the bookinfo namespace again:

```
kubectl apply -f demo1/sleep-in-bookinfo-ns.yaml

kubectl exec -it deploy/sleep -- sh
```

Try accessing the reviews & ratings APIs:

```
curl http://reviews:9080/1

curl http://ratings:9080/ratings/1
```

## Configure access for all services

You can use a fine-grained model to describe the access control between application components:

- [bookinfo-workload-authorization-policies.yaml](bookinfo-workload-authorization-policies.yaml)

```
kubectl get po -n istio-ingress -o json | jq '.items[0].spec.serviceAccountName'

kubectl apply -f ./demo2/bookinfo-workload-authorization-policies.yaml
```

> Check http://localhost/productpage

Check service access from the rogue Pod:

```
kubectl exec -it deploy/sleep -- sh

curl http://reviews:9080/1

curl http://ratings:9080/ratings/1

curl http://details:9080/details/1

curl http://productpage:9080
```

Or the same result can be obtained with a coarse-grained model, targetting the whole namespace:

- [bookinfo-namespace-authorization-policies.yaml](bookinfo-namespace-authorization-policies.yaml)

```
kubectl delete authorizationpolicy --all

kubectl apply -f demo2/bookinfo-namespace-authorization-policies.yaml
```

```
kubectl exec -it deploy/sleep -- curl http://ratings:9080/ratings/1
```

> Check http://localhost/productpage

## Security in-depth

Authorization rules depend on the Service Account of the Pod. If a user has access to the resources, they can deploy a rogue app using a permitted Service Account:

- [sleep-with-productpage-sa.yaml](sleep-with-productpage-sa.yaml)

```
kubectl apply -f demo2/sleep-with-productpage-sa.yaml
```

This new rogue Pod is using the identity of the product page, so it does have access:

```
kubectl exec -it deploy/sleep2 -- curl http://ratings:9080/ratings/1
```