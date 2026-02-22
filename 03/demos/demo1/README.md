# Dark Launch - Releasing a New Feature

In this demo we launch new version of the reviews service and use Istio to manage traffic so only the test user is directed to the new version, and then deliberately inject faults in the network.

## References

- [Istio fault injection docs](https://istio.io/latest/docs/tasks/traffic-management/fault-injection/)

- [HTTPFaultInjection spec](https://istio.io/latest/docs/reference/config/networking/virtual-service/#HTTPFaultInjection)


## Pre-reqs

You'll need Istio, the Istio Gateway and the Bookinfo app running (from demo 1 and demo 2 in the `m2` folder).


## Run Book Info

We'll start by deploying the demo app with a standard Istio setup:

- [01-namespace.yaml](./bookinfo/01-namespace.yaml) - namespace with istio injection
- [02-bookinfo.yaml](./bookinfo/02-bookinfo.yaml) - same spec as m2
- [03-bookinfo-gateway.yaml](./bookinfo/03-bookinfo-gateway.yaml) - same as M2

```
kubectl apply -f demo1/bookinfo
```

> Check at http://localhost/productpage


## Deploy a new version of the Reviews service

v2 of the service adds a rating section to the UI:

- [reviews-v2.yaml](./reviews-v2.yaml) - Deployment object where Pods have a version=v2 label

```
kubectl apply -f demo1/reviews-v2.yaml
```

Check deployment:

```
kubectl get pods -l app=reviews --show-labels

kubectl describe svc reviews

kubectl get endpoints -l app=reviews
```

> Browse to http://localhost/productpage and refresh, requests load-balanced between v1 and v2

New service shows star rating, but it's 50-50 whether you see it.

## Route testers to v2 with Istio

We shape the traffic using DestinationRules and VirtualServices:

- [reviews-v2-tester.yaml](./reviews-v2-tester.yaml) - defines subsets and routes test user to v2

```
kubectl apply -f demo1/reviews-v2-tester.yaml

kubectl describe virtualservice reviews
```

> Browse to http://localhost/productpage - unknown users always see v1 

Login as `tester` who always sees v2


## Test with network delay

Routes in VirtualServices can include deliberate faults:

[reviews-v2-tester-delay.yaml](./reviews-v2-tester-delay.yaml) - adds a 2.5 second delay to all requests

```
kubectl apply -f demo1/reviews-v2-tester-delay.yaml
```

> Browse to http://localhost/productpage - `tester` gets delayed response, all others OK

## Test with service fault

Fault injection can be with delays or aborts:

[reviews-v2-tester-503.yaml](./reviews-v2-tester-503.yaml) - returns with a 503 error response for 50% of requests

```
kubectl apply -f demo1/reviews-v2-tester-503.yaml
```

> Browse to http://localhost/productpage -  `tester` gets 50% failures, all others OK