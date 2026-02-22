# Deploying Istio to Kubernetes in the cloud

Deploy a production-grade Istio setup on a cluster with an existing app, then run Istio and non-Istio apps together.

## Reference

- [Istio's platform setup guides](https://istio.io/docs/setup/platform-setup/)

- [Installing Istio with `istioctl`](https://istio.io/latest/docs/setup/install/istioctl/)

## Setup

I'm using a [4-node AKS cluster](create-aks-cluster.md).

Check you're using the right cluster:

```
kubectl get nodes
```

Deploy random number generator app:

```
kubectl apply -f demo1/rng/

kubectl get pods -n rng

kubectl get svc -n rng

kubectl describe svc -n rng numbers-web

dig rng.sixeyed.com
```

> Check the app at http://rng.sixeyed.com


## Istio default profiles

> Download [istioctl](https://github.com/istio/istio/releases)

```
istioctl version

istioctl profile list

istioctl profile dump > demo1/temp/istio-default-profile.yaml
``` 

Examine the default profile settings:

- [istio-default-profile.yaml](temp/istio-default-profile.yaml)

> These are the [IstioOperator configurable settings](https://istio.io/latest/docs/reference/config/istio.operator.v1alpha1/)

Deploy direct from `istioctl`:

```
istioctl manifest apply

kubectl get po,svc -n istio-system

kubectl describe svc istio-ingressgateway -n istio-system
```

Customize the deployment with a DNS annotation for the ingress gateway:

```
istioctl manifest apply --set 'values.gateways.istio-ingressgateway.serviceAnnotations.service\.beta\.kubernetes\.io/azure-dns-label-name=gatewaysixeyedcom'

kubectl get svc -n istio-system

kubectl describe svc istio-ingressgateway -n istio-system
```

> Check [random number generator app](http://rng.sixeyed.com) is still working

## Customizing the installation

Generate a Kube manifest:

```
istioctl manifest generate -o demo1/temp/istio-default
```

Explore the profile manifests:

- [Base.yaml](temp/istio-default/Base/Base.yaml) - CRDs and Service Account
- [Pilot.yaml](temp/istio-default/Base/Pilot/Pilot.yaml) - Istiod Deployment and configuration
- [IngressGateways.yaml](temp/istio-default/Base/Pilot/IngressGateways/IngressGateways.yaml) - gateway Deployment and configuration

Generate a custom manifest with override:

```
istioctl manifest generate -o demo1/temp/istio-default --set 'values.gateways.istio-ingressgateway.serviceAnnotations.service\.beta\.kubernetes\.io/azure-dns-label-name=gatewaysixeyedcom'
```

Or use an [override file](istio-override/ingress-dnslabel.yaml) for more control:

```
istioctl manifest generate -o demo1/temp/istio-custom -f demo1/istio-override/ingress-dnslabel.yaml
```

- Open [customized IngressGateway manifest](temp/istio-custom/Base/Pilot/IngressGateways/IngressGateways.yaml)
- Search for _type: LoadBalancer_
- Verify the annotation is applied

## Deploy BookInfo

Use a standard BookInfo v1 deployment with Istio:

- [01_ns.yaml](bookinfo/01_ns.yaml) - namespace with Istio injection
- [auth.yaml](bookinfo/auth.yaml) - mTLS and authorization policies
- [ingress.yaml](bookinfo/ingress.yaml) - shared Gateway and app VirtualService

```
kubectl apply -f demo1/bookinfo/
```

> Added CNAME to DNS record - `bookinfo.sixeyed.com` pointing to Azure DNS label

```
dig bookinfo.sixeyed.com
```

> Check [bookinfo.sixeyed.com](http://bookinfo.sixeyed.com) is up

The original app uses its own Azure DNS CNAME:

```
dig rng.sixeyed.com
```

> Check [rng.sixeyed.com](http://rng.sixeyed.com) is still working

## Install Kiali

_Add-ons used to be availabe as options in the Operator, but now they are separate manifests - e.g. https://github.com/istio/istio/blob/master/samples/addons/kiali.yaml_

Deploy Prometheus and Kiali as we have before, but with no VirtualService:

```
kubectl apply -f demo1/kiali

kubectl get svc -n istio-system
```

We can use `istioctl` to access the UI, which is effectively a simple port forward from localhost:

```
istioctl dashboard kiali
```
