# Distributed Tracing

Distributed tracing lets you model the dependency tree for a single incoming request. In this demo we'll use [Jaeger](https://www.jaegertracing.io) to visualize trace spans and investigate outliers.

## Reference

- Istio integration for [Jaeger](https://istio.io/latest/docs/ops/integrations/jaeger/)

- [Configuring telemetry in Istio](https://istio.io/latest/docs/tasks/observability/telemetry/)

## Pre-reqs

Follow the steps from [Demo 1](../demo1/README.md) to deploy a working instance of Bookinfo with Istio, Prometheus and Kiali configured.

```
kubectl config set-context --current --namespace=bookinfo
```

Remove the fault injection from the reviews component:

```
kubectl delete virtualservice reviews
```

## Publish the Jaeger UI

Deploy the Jaeger integration & routing:

- [jaeger.yaml](jaeger/jaeger.yaml) - Deployment and Services for Jaeger

- [virtualservice.yaml](jaeger/virtualservice.yaml) - binding for `jaeger.local` to the shared Gateway

```
kubectl apply -f demo3/jaeger/
```

Istio is already configured to use Zipkin for tracing:

```
kubectl describe cm istio -n istio-system
```

But we want to use an explicitly named provider, which lets us configure the tracing for each application namespace:

- [istio-config-zipkin.yaml](istio-config-zipkin.yaml) - adds a provider called `zipkin` for trace collection

```
kubectl apply -f demo3/istio-config-zipkin.yaml
```

> Browse to http://jaeger.local 

Only Jaeger has traces (it monitors itself).

> Browse to http://bookinfo.local/productpage & refresh a few times (the default sampling rate is 1% so not many requests get stored)

- Select service `productpage.bookinfo`
- Follow traces - some with details & reviews, some with ratings too
- Zoom into timeline & check tags

## Increase sampling rate

The telemetry API is for configuring behaviour at different levels (mesh, namespace, workload):

- [bookinfo-tracing.yaml](bookinfo-tracing.yaml) - sets all proxies in the bookinfo namespace to send 100% of requests to zipkin

```
kubectl apply -f demo3/bookinfo-tracing.yaml
```

Generate some load:

```
docker container run `
  --add-host "bookinfo.local:192.168.2.120" `
  fortio/fortio `
  load -c 32 -qps 100 -t 60s `
  http://bookinfo.local/productpage
```

> Browse to http://jaeger.local 

## Track down a bad update

Apply an update from the dev team which slows down the whole app:

```
kubectl apply -f demo3/mysterious-delay.yaml | Out-Null
```

> Check http://bookinfo.local/productpage & refresh

- Back to Jaegar
- Follow trace for outlier

Open http://kiali.local & find logs for slow service

An alternative APM (Application Performance Monitor) with Istio integration is [Apache SkyWalking](https://skywalking.apache.org). It has too many features to cover here, but it essentially combines multiple views into one UI.

> Check the SkyWalking demo site: http://demo.skywalking.apache.org/ (login: `skywalking` / `skywalking`)