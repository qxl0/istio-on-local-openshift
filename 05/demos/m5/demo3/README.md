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

> OpenShift Local: http://jaeger-istio-gateway.apps-crc.testing

Only Jaeger has traces (it monitors itself).

> OpenShift Local: http://bookinfo-istio-gateway.apps-crc.testing/productpage (refresh a few times - the default sampling rate is 1% so not many requests get stored)

- Select service `productpage.bookinfo`
- Follow traces - some with details & reviews, some with ratings too
- Zoom into timeline & check tags

## Increase sampling rate

The telemetry API is for configuring behaviour at different levels (mesh, namespace, workload):

- [bookinfo-tracing.yaml](bookinfo-tracing.yaml) - sets all proxies in the bookinfo namespace to send 100% of requests to zipkin

```
kubectl apply -f demo3/bookinfo-tracing.yaml
```

Generate some load using Fortio inside the cluster:

```powershell
oc -n bookinfo exec -it pod/fortio -- \
  fortio load -c 32 -qps 100 -t 60s \
  http://productpage:9080/productpage
```

> OpenShift Local: http://jaeger-istio-gateway.apps-crc.testing 

## Track down a bad update

Apply an update from the dev team which slows down the whole app:

```
kubectl apply -f demo3/mysterious-delay.yaml | Out-Null
```

> Check http://bookinfo.local/productpage & refresh

- OpenShift Local: http://bookinfo-istio-gateway.apps-crc.testing/productpage (refresh to generate traces)

- Back to Jaegar (http://jaeger-istio-gateway.apps-crc.testing)
- Follow trace for outlier

- Open Kiali (http://kiali-istio-gateway.apps-crc.testing)lication Performance Monitor) with Istio integration is [Apache SkyWalking](https://skywalking.apache.org). It has too many features to cover here, but it essentially combines multiple views into one UI.

> Check the SkyWalking demo site: http://demo.skywalking.apache.org/ (login: `skywalking` / `skywalking`)