# Running the Bookinfo sample app

In this demo we'll run the Bookinfo app with and without Istio

## References

- [Istio Bookinfo Sample Application](https://istio.io/latest/docs/examples/bookinfo/)

- [Bookinfo source code](https://github.com/istio/istio/tree/master/samples/bookinfo)

## Pre-requisites

You'll need Istio and the Istio Gateway deployed (from demo 1).

## Deploy bookinfo

We'll create a dedicated namespace for the app, and switch to that namespace:

```
oc create namespace bookinfo

oc config set-context --current --namespace bookinfo
```

There are multiple components in the app, we have them all in a single YAML file:

- [bookinfo.yaml](./bookinfo.yaml) - Kubernetes model for the app


Now we'll deploy the app:

```
oc apply -f demo2/bookinfo.yaml
```

> Kubernetes creates standard resources - Services, ServiceAccounts & Deployments

Check the Pods:

```
oc get po
```

And when they're all ready we can get the address for the Product Page Service:

```
oc get svc
```

> On Docker Desktop you can browse to http://localhost:9080

## Deploy with Istio

Now we'll deploy the same app with Istio, starting by clearing down the previous deployment:

```
oc delete -f demo2/bookinfo.yaml
```

And we'll add the Istio label to the namespace, so Pods have the proxy containers injected:

```
oc label namespace bookinfo istio-injection=enabled
```

Then we can deploy the same manifests:

```
oc apply -f demo2/bookinfo.yaml
```

Now each Pod has two containers - one for the app component, and one for the proxy:

```
oc get po 

oc logs -l app=productpage -c istio-proxy
```

> The proxy logs just show the Istio configuration being loaded and applied

## Add Gateway

We can still access our app from the LoadBalancer Service, but if we want all the features of Istio we should use it for external traffic too.

We do that with a Gateway and a VirtualService:

- [bookinfo-gateway.yaml](./bookinfo-gateway.yaml)

Those are Istio resources, but they get deployed in the usual way:

```
oc apply -f demo2/bookinfo-gateway.yaml
```

The Istio Gateway is listening on port 80, we can try the app at http://localhost

> Nothing. 

But we can check the 404 is coming from Istio:

```
curl localhost -v
```

Look at the routing again in [VirtualService](./bookinfo-gateway.yaml). The routing rules use matches with specific URL paths.

Try http://localhost/productpage

> Aha!
