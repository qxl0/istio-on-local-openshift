# Logging Access Requests

Istio proxies can be configured to emit network access logs. In this demo we'll see how to do that and collect the logs in a typical EFK logging stack - [Elasticsearch](https://www.elastic.co/elasticsearch/), [Fluentbit](https://docs.fluentbit.io/manual/) and [Kibana](https://www.elastic.co/kibana).

## Reference

- [Configuring access logging in Istio](https://istio.io/latest/docs/tasks/observability/logs/access-log/)

- [Walkthrough of EFK deplyment in Kubernetes](https://kubernetes.courselabs.co/labs/logging/
)
## Pre-reqs

Follow the steps from [Demo 1](../demo1/README.md) to deploy a working instance of Bookinfo with Istio, Prometheus and Kiali configured.

```
kubectl config set-context --current --namespace=bookinfo
```

Replace the slow details service:

```
kubectl apply -f demo1/bookinfo/
```

## Configure proxy logging

There is already some logging happening:

> Browse to http://bookinfo.local/productpage

```
kubectl logs -l app=productpage -c app

kubectl logs -l app=productpage -c istio-proxy
```

The proxy logs only show changes to Envoy configuration. We can enable access logs with a change to the Istio config:

- [istio-config-accessLog.yaml](istio-config-accessLog.yaml) - turns on access logs for proxy containers, directing them to stdout

```
kubectl apply -f demo4/istio-config-accessLog.yaml
```

> Browse to http://bookinfo.local/productpage

```
kubectl logs -l app=productpage -c istio-proxy
```

Now we see access logs in a standard(ish) web server format. We can switch to structured logging with another config change:

- [istio-config-accessLog-json.yaml](istio-config-accessLog-json.yaml) - writes access logs in JSON

```
kubectl apply -f demo4/istio-config-accessLog-json.yaml
```

> Browse to http://bookinfo.local/productpage

Now the proxy logs are in JSON:

```
kubectl logs -l app=productpage -c istio-proxy
```

For all Istio-managed components:

```
kubectl logs -l app=details -c istio-proxy
```

## Deploy EFK logging stack

Logging frameworks run in the cluster and collectm all the container logs. This is a minimal dev deployment of EFK:

- [elasticsearch.yaml](./logging/elasticsearch.yaml) - Deployment and Service for log storage
- [kibana.yaml](./logging/kibana.yaml) - Deployment and Service for web UI
- [fluentbit.yaml](./logging/fluentbit.yaml) - DaemonSet and RBAC for log collection

We'll use a Fluentbit configuration which separates app logs, proxy logs & system logs into different Elasticsearch indices:

- [fluentbit.yaml](./logging/fluentbit-config.yaml)

```
kubectl apply -f demo4/logging/

kubectl get pods -n logging -w

kubectl get svc -n logging
```

> Browse to http://bookinfo.local/productpage

> Browse to Kibana at http://localhost:5601

- In _Stack Management_ create index pattern for `proxy*`
- In _Discover_ browse the logs
- Filter by `app` label

## Generate some load

Run Fortio for 30 seconds:

_Use your own host's IP address_

```
docker container run `
  --add-host "bookinfo.local:192.168.2.120" `
  fortio/fortio `
  load -c 32 -qps 25 -t 30s http://bookinfo.local/productpage
```

- Back to Kibana
- Drill down into the new block of logs
