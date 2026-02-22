# Securing Services with Mutual TLS

In this demo we apply encryption and authentication between traffic in Istio-managed Pods using mutual TLS - HTTPS transport requiring a client certificate.

## References

- [Istio mutual TLS docs](https://istio.io/latest/docs/concepts/security/#mutual-tls-authentication)

- [PeerAuthentication spec](https://istio.io/latest/docs/reference/config/security/peer_authentication/)

## Pre-reqs

Deploy Istio & gateway:

```
helm upgrade --install istio-base istio/base -n istio-system --create-namespace

helm upgrade --install istiod istio/istiod -n istio-system

helm upgrade --install istio-ingress istio/gateway -n istio-ingress --create-namespace
```

## Deploy and access the details API

Deploy bookinfo and a legacy app which is running outside of Istio:

- [sleep-in-legacy-ns.yaml](sleep-in-legacy-ns.yaml)

```
kubectl apply -f ./demo1/bookinfo/ -f ./demo1/sleep-in-legacy-ns.yaml
```

Run a shell in the Product Page Pod container:

```
kubectl -n bookinfo exec -it deploy/productpage-v1 -- python
```

```
import urllib.request
urllib.request.urlopen("http://details:9080/details/1").read()
exit()
```

Run a shell in the legacy Pod container:

```
kubectl -n legacy get po

kubectl -n legacy exec -it deploy/sleep -- sh
```

Use the details API:

```
nslookup details.bookinfo.svc.cluster.local

curl http://details.bookinfo.svc.cluster.local:9080/details/1

curl http://details.bookinfo.svc.cluster.local:9080/details/100
```

## Require mutual TLS for bookinfo services

> In a new session

Enforce mTLS for all services in the bookinfo namespace:

- [mutual-tls.yaml](mutual-tls.yaml):

```
kubectl apply -f demo1/mutual-tls.yaml
```

> Back to the sleep container session

Check the API again:

```
curl http://details.bookinfo.svc.cluster.local:9080/details/100
```

> Fails, because the server requires a TLS cert

The components in the bookinfo namespace can still communicate - check http://localhost/productpage


## Accessing services from the Istio Proxy

Shell into the product page app container:

```
kubectl -n bookinfo exec -it deploy/productpage-v1 -c productpage -- python
```

Fetch details with HTTP:

```
import urllib.request; 

urllib.request.urlopen("http://details:9080/details/1").read()

exit()
```

Shell into the product page proxy container:

```
kubectl -n bookinfo exec -it deploy/productpage-v1 -c istio-proxy --  bash
```

Try to access the details API:

```
curl http://details:9080/details/100

curl https://details:9080/details/100

curl -k https://details:9080/details/100
```

> Fails: `alert certificate required`

Certs are distributed securely using Envoy SDS (Secret Discovery Service), not stored on the local disk:

```
ls /etc/certs

exit
```

```
kubectl logs -l app=productpage -c istio-proxy
```

## Gain Peer Authentication by moving Namespace

Peer authentication requires a valid client certificate, but without authorization that allows be any client in the mesh. We can recreate our legacy app in the bookinfo namespace:

- [sleep-in-bookinfo-ns.yaml](sleep-in-bookinfo-ns.yaml)


```
kubectl apply -f demo1/sleep-in-bookinfo-ns.yaml
```

The new sleep Pod is an Istio-enabled namespace, so it has the proxy container:

```
kubectl get po 
```

And now we can use the details API:

```
kubectl -n bookinfo exec -it deploy/sleep -- sh

curl http://details.bookinfo.svc.cluster.local:9080/details/100
```

We need security in depth, not individual pieces of security.