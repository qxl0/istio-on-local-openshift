# Visualizing the Service Mesh

[Kiali](https://kiali.io) is a visualization UI which shows the live service graph and real time traffic to your application components. In this demo we'll see how the data from Istio powers insight into app communication.

## Reference

- Istio add-ons - [Prometheus](https://istio.io/latest/docs/ops/integrations/prometheus/#option-1-quick-start) and [Kiali](https://istio.io/latest/docs/ops/integrations/kiali/)

- [Configuring Kiali for Istio](https://kiali.io/docs/configuration/istio/)

## Pre-reqs

Deploy Istio & ingress gateway:

```
helm upgrade --install istio-base istio/base -n istio-system --create-namespace

helm upgrade --install istiod istio/istiod -n istio-system

helm upgrade --install istio-ingress istio/gateway -n istio-ingress --create-namespace
```


## Deploy Bookinfo

We'll be exposing several UIs through Istio, this gateway is configured to work with VirtualServices in any namespace, with any host ending `.local`:

- [gateway.yaml](gateway.yaml) - using a single Gateway for multiple apps

```
kubectl apply -f demo1/gateway.yaml
```

Deploy the Bookinfo app, this is the usual setup except the VirtualService explicitly references the shared Gateway:

- [virtualservice.yaml](bookinfo/virtualservice.yaml) - binding `bookinfo.local` to the shared Gateway in another namespace

```
kubectl apply -f demo1/bookinfo/
```

Bookinfo and the other UIs are in my hosts file:

```
127.0.0.1  bookinfo.local
127.0.0.1  test.bookinfo.local
127.0.0.1  kiali.local
127.0.0.1  prometheus.local
127.0.0.1  grafana.local
127.0.0.1  jaeger.local
```

> Browse to http://bookinfo.local/productpage

## Deploy Kiali

Kiali reads metrics from Prometheus. Istio has ready-made integrations to set them both up:

- [01-prometheus.yaml](kiali/01-prometheus.yaml) - Deployment, Service, configuration and RBAC for Prometheus

- [02-kiali.yaml](kiali/02-kiali.yaml) - Deployment, Service, configuration and RBAC for Kiali

- [03-virtualservice.yaml](kiali/03-virtualservice.yaml) - VirtualService binding `kiali.local` to the shared Gateway

```
kubectl apply -f demo1/kiali/

kubectl get po -n istio-system -w
```

> Browse to http://kiali.local

- App graph in _Graph_
- Set to replay _Last 5m_
- Select namespace _bookinfo_
- Check `productpage` in Kiali _Workloads_

## Monitoring canary deployments

Deploy updated components:

- [v2 product page](v2/productpage-v2-canary.yaml) - with 70/30 split
- [v2 reviews API](v2/reviews-v2-canary.yaml) - with 60/40 split

```
kubectl apply -f demo1/v2/
```

> Browse to http://bookinfo.local/productpage and refresh 

- Back to Kiali
- Switch versioned app graph
- Add _Traffic distribution_ label
- Check bookinfo virtual service in _Istio Config_ (editable!)

## Generate some load

Use [Fortio](https://fortio.org) to send a few hundred requests to the app:

Create fortio pod:
```powershell
oc apply -f .\fortio-pod.yaml
oc -n bookinfo wait pod/fortio --for=condition=Ready --timeout=120s
oc -n bookinfo get pod fortio -o wide
```

_Inject your host IP address into the container_

```
oc -n bookinfo exec -it pod/fortio -- `
  fortio load -c 32 -qps 25 -t 60s `
  http://productpage:9080/productpage

- Back to Kiali _Graph_
- Add _Response time_
- Add _Traffic Rate_