# Using Istio for fault-tolerance

In this demo we'll see some of the power of Istio, using it to repair a broken application.

## References

- [Istio Traffic Management docs](https://istio.io/latest/docs/tasks/traffic-management/)

- [VirtualService spec](https://istio.io/latest/docs/reference/config/networking/virtual-service/)

## Pre-requisites

You'll need Istio, the Istio Gateway and the Bookinfo app running (from demo 1 and demo 2).


## Route traffic through Istio

The Bookinfo app is already running through Istio. Now we'll use Istio for traffic management with one component:

- [details-virtualservice.yaml](./details-virtualservice.yaml) - a VirtualService for the details component

Deploy the VirtualService:

```
kubectl apply -f demo3/details-virtualservice.yaml

kubectl describe vs details
```

> Browse to http://localhost/productpage - it's the same :)

## Deploy a bad update to the service

We have a new version of the details component to deploy:

- [details-bad-release.yaml](./details-bad-release.yaml) - adds a feature flag which causes the component to time-out

Run the update:

```
kubectl apply -f demo3/details-bad-release.yaml
```

And watch the logs for the new details component:

```
kubectl logs -f -l app=details -c details
```

> Browse to http://localhost/productpage - details call times out after 30 seconds

## Update VirtualService with timeout

Istio is managing the traffic from the product page to the details component. We can set a shorter network timeout than the app uses:

- [details-virtualservice-timeout.yaml](./details-virtualservice-timeout.yaml) - sets a network timeout of 5 seconds

Apply the update:

```
kubectl apply -f demo3/details-virtualservice-timeout.yaml
```

And check this is just an Istio config change, the same Pods are running:

```
kubectl get po
```

> Browse to http://localhost/productpage - page now responds after 5 seconds

## Update VirtualService with retry

This particular failure only happens with the first call to the service. We can configure Istio to automatically retry on failures or timeouts:

- [details-virtualservice-retry.yaml](./details-virtualservice-retry.yaml) - retry after 2 seconds or on 500 errors

Update the VirtualService spec:

```
kubectl apply -f demo3/details-virtualservice-retry.yaml
```

Check these are still the same application Pods:

```
kubectl get po
```

And watch the logs again:
```

kubectl logs -f -l app=details -c details
```

> Browse to http://localhost/productpage - details call times out and then automatically retries

The app is working correctly again (with some time lag).