# Blue/Green Deployment

In this demo we run two versions of the application homepage on different domain names, and switch the domain between live and test versions.

## References

- [Istio request routing docs](https://istio.io/latest/docs/tasks/traffic-management/request-routing/)

- [Destination spec](https://istio.io/latest/docs/reference/config/networking/virtual-service/#Destination)

## Pre-reqs

You'll need Istio, the Istio Gateway and the Bookinfo app running (from demo 1 and demo 2 in the `m2` folder), and the BookInfo app running from demo 1 in this module:

```
kubectl apply -f demo1/bookinfo
```

Check the Pods are all running:

```
kubectl config set-context --current --namespace=bookinfo

kubectl get pods -o wide 
```

## Deploy v2 homepage

This is an external component so we need an Istio Gateway to receive traffic:

```
kubectl get gateway bookinfo-gateway -o yaml
```

Deploy v2 product page:

- [productpage-v2.yaml](./productpage-v2.yaml) - Deployment, DestinationRule and VirtualServices with live & test domains

```
kubectl apply -f demo2/productpage-v2.yaml
```

Check deployment:

```
kubectl get pods -l app=productpage --show-labels

kubectl describe virtualservice bookinfo

kubectl describe virtualservice bookinfo-test
```

Add `bookinfo.local` domains to hosts file:

```
cat /etc/hosts

# on Windows add to C:\Windows\System32\drivers\etc\hosts
```

> Browse to live v1 set at http://bookinfo.local/productpage

> Browse to test v2 site at http://test.bookinfo.local/productpage


## Blue/green deployment - flip to v2 

Switch vertsions for test and live:

- [productpage-test-to-live.yaml](./productpage-test-to-live.yaml) - flips the subset for each VirtualService

```
kubectl apply -f demo2/productpage-test-to-live.yaml
```

Check live deployment:

```
kubectl describe virtualservice bookinfo
```

> Live is now v2 at http://bookinfo.local/productpage

Check test deployment:

```
kubectl describe virtualservice bookinfo-test
```

> Test is now v1 at http://test.bookinfo.local/productpage

## Blue/green deployment - flip back to v1

Reverting the deployment just requires a change to the subsets again:

- [productpage-live-to-test.yaml](./productpage-live-to-test.yaml) - uses the v1 subset for live and v2 for test

```
kubectl apply -f demo2/productpage-live-to-test.yaml
```

> Live is back to v1 http://bookinfo.local/productpage

> Test is back to v2 http://test.bookinfo.local/productpage
