# Migrating an Existing App to Istio

Bring the existing random-number app into the mesh, without breaking it.

## Add services to the mesh with permissive mTLS

Check Istio auto-injection:

```
kubectl get ns -L istio-injection
```

Generate manifests with side-car config:

```
istioctl kube-inject -f demo1/rng/02_numbers-api.yaml -o demo2/temp/numbers-api.yaml
```

> Compare injected config with setup

A cleaner option is to add labels to the Pod spec. Istio takes care of auto-injection, but you can control the rollout order:

- [02_numbers-api.yaml](rng/02_numbers-api.yaml) - API service with Istio inject label
- [03_numbers-web.yaml](rng/03_numbers-web.yaml) - and the web component

First we want to ensure the Pods use permissive TLS when they join the mesh:

- [01_auth.yaml](rng/01_auth.yaml) - PeerAuthentication default for the namespace

```
kubectl apply -f demo2/rng/01_auth.yaml
```

Add API Pods to the mesh - they will support but not demand mTLS:

```
kubectl apply -f demo2/rng/02_numbers-api.yaml

kubectl -n rng wait --for=condition=Ready pod -l app=numbers-api,sidecar.istio.io/inject=true
```

> Test the app at http://rng.sixeyed.com

Add web Pods to the mesh - now web to API calls will be mTLS:

```
kubectl apply -f demo2/rng/03_numbers-web.yaml

kubectl -n rng wait --for=condition=Ready pod -l app=numbers-web,sidecar.istio.io/inject=true

kubectl get po -n rng
```

> Test the app at http://rng.sixeyed.com

## Enforce mTLS

Set mTLS to strict and add an AuthorizationPolicy:

- [rng-mtls/01_auth.yaml](rng-mtls/01_auth.yaml) - PeerAuthentication with strict mTLS

```
kubectl apply -f demo2/rng-mtls/01_auth.yaml
```

> Test at http://rng.sixeyed.com

This **FAILS** - there is  no mTLS from the Kubernetes `LoadBalancer` service to the web Pods, which require it.

Revert back to permissive mTLS:

```
kubectl apply -f demo2/rng/01_auth.yaml
```

> Fixes http://rng.sixeyed.com

## Ingress migration

Instead we need to route external traffic into the app via Istio. Add a gateway VirtualService for the RNG app:

- [00_ingress.yaml](rng-mtls/00_ingress.yaml) - binds the DNS name for the app to the shared gateway

```
kubectl apply -f demo2/rng-mtls/00_ingress.yaml
```

Now the app is available through Istio's ingress. Find the ingress IP address:

```
kubectl get svc istio-ingressgateway -n istio-system

kubectl get svc istio-ingressgateway -n istio-system -o=jsonpath='{.status.loadBalancer.ingress[0].ip}'

$ip=$(kubectl get svc istio-ingressgateway -n istio-system -o=jsonpath='{.status.loadBalancer.ingress[0].ip}')
```

And by making a GET request with the correct domain name in the host header, we can verify the app with cURL:

```
curl --header 'Host: rng.sixeyed.com' http://$ip
```

> Change DNS CNAME record for `rng.sixeyed.com`

```
dig rng.sixeyed.com

dig bookinfo.sixeyed.com
```

> Test http://rng.sixeyed.com

Now it's all working, we can switch to required mTLS again:

```
kubectl apply -f demo2/rng-mtls/01_auth.yaml

kubectl describe peerauthentication default -n rng
```

> Test http://rng.sixeyed.com and http://bookinfo.sixeyed.com

Lastly change the original LoadBalancer Service to an internal type:

- [02_service.yaml](rng-mtls/02_service.yaml) - uses ClusterIP for the Service, which is only used by the Istio gateway

```
kubectl apply -f demo2/rng-mtls/02_service.yaml --force

kubectl get svc -n rng
```

(`force` is needed to change the service type)

> Test http://rng.sixeyed.com 


Check in Kiali:

```
istioctl dashboard kiali
```
