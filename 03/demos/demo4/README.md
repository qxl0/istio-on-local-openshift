# Circuit Breaker

In this demo we add outlier detection to a load-balanced service so unhealthy endpoints are removed for a period of time and don't receive any more requests.

## References

- [Istio circuit breaker docs](https://istio.io/latest/docs/tasks/traffic-management/circuit-breaking/)

- [OutlierDetection spec](https://istio.io/latest/docs/reference/config/networking/destination-rule/#OutlierDetection)

## Pre-reqs

You'll need Istio, the Istio Gateway and the Bookinfo app running (from demo 1 and demo 2 in the `m2` folder), and reset to the original BookInfo app from demo 1:

```
kubectl delete ns bookinfo

kubectl apply -f demo1/bookinfo
```

Verify the app components:

```
kubectl get pods
```

> Check at http://localhost/productpage 

## Deploy updated Details service

Run a new version of the service with custom configuration & scale:

- [details-v2.yaml](./details-v2.yaml) - 4 Pods running with a 50% unhealthy flag

```
kubectl apply -f demo4/details-v2.yaml
```

Check deployment:

```
kubectl describe service details

kubectl describe vs details

kubectl describe dr details
```

Some of the replicas will be unhealthy from the start:

```
kubectl logs -l app=details,version=v2 -c details
```

> Browse to http://localhost/productpage & refresh lots. Up to 50% of details call fail.

Check logs:

```
kubectl logs -l app=details,version=v2 -c details
```

## Apply circuit breaker

Outlier detection ensures unresponsive services get a break from client requests:

- [details-circuit-breaker.yaml](./details-circuit-breaker.yaml) - updated rules with outlier detection

```
kubectl apply -f demo4/details-circuit-breaker.yaml
```

Check deployment:

```
kubectl describe dr details

kubectl get po -l app=details,version=v2
```

> Browse to http://localhost/productpage & refresh lots. As pods return errors they get excluded - after a while there are no errors, requests only go to healthy pods.

Wait 5 minutes and the unhealthy Pods return to the subset - but will then get excluded again when they fail twice.