# Canary Deployment

In this demo we run two versions of the application homepage and split the traffic between them - using a canary release pattern where the new version gradually gets more traffic.

## References

- [Istio traffic shifting docs](https://istio.io/latest/docs/tasks/traffic-management/traffic-shifting/)

- [HTTPRouteDestination spec](https://istio.io/latest/docs/reference/config/networking/virtual-service/#HTTPRouteDestination)

## Pre-reqs

You'll need Istio, the Istio Gateway and the Bookinfo app running (from demo 1 and demo 2 in the `m2` folder), and the BookInfo app from demo 1 and demo 2 in this module:

```
kubectl apply -f demo1/bookinfo

kubectl apply -f demo2/productpage-v2.yaml
```

> Check at http://bookinfo.local/productpage

## Canary with 30% traffic to v2

The v1 and v1 homepage Pods are running, but all the traffic is going to v1:

```
kubectl get po,gateway,virtualservice
```

The spec defines a canary deployment, with a 70/30 traffic split:

- [productpage-canary-70-30.yaml](./productpage-canary-70-30.yaml)

```
kubectl apply -f demo3/productpage-canary-70-30.yaml
```

Check deployment:

```
kubectl describe virtualservice bookinfo-test

kubectl describe virtualservice bookinfo
```

> Browse to http://test.bookinfo.local/productpage & refresh, always v2

> Browse to http://bookinfo.local/productpage & refresh, mostly v1 responses with some v2

## Ongoing canary rollout

Shift traffic to v2:

- [productpage-canary-55-45.yaml](./productpage-canary-55-45.yaml) - 55/45 split
- [productpage-canary-25-75.yaml](./productpage-canary-25-75.yaml) - 25/75 split

```
kubectl apply -f demo3/productpage-canary-25-75.yaml
```

Final split:

- [0/100 split](productpage-canary-0-100.yaml)

## Canary with sticky sessions

Open network tab in developer tools and refresh till you see v2 - there's a cookie set from the v2 product page.

We can use that cookie to make sure users who have seen v2 continue to see v2:

- [productpage-canary-with-cookie.yaml](./productpage-canary-with-cookie.yaml) - canary split with with cookie

```
kubectl apply -f demo3/productpage-canary-with-cookie.yaml
```

> Open new private window at http://bookinfo.local/productpage & refresh - once you hit v2 you'll always get v2

Check by disabling cookies:

_e.g. in Firefox_
- about:preferences
- Search "cookies"
- Set to custom
- Block all cookies

> Browse to http://bookinfo.local/productpage & refresh - back to 70/30 split