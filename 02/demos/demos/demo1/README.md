# Installing Istio

In this demo we'll install Istio on Kubernetes, and see what it does to our application Pods.

## Reference

- [Getting Started with Istio](https://istio.io/latest/docs/setup/getting-started/)

- [Installing Istio with Helm](https://istio.io/latest/docs/setup/install/helm/)

- [Istio manifests on GitHub](https://github.com/istio/istio/tree/master/manifests/charts)

## Pre-requisites

Access to a Kubernetes cluster where you have cluster-admin permissions (a local cluster is fine).

## Deploy Istio

There are several installation options, including the Istio command line tool. We'll start with Helm, which is the package manager for Kubernetes apps.

I'm using [Docker Desktop](https://www.docker.com/products/docker-desktop/) to run Kubernetes locally. On the Mac I've configured the engine with 4 CPUs and 16GB RAM - which is plenty for Istio.

First I'll check I'm connected to the right cluster:

```
kubectl get nodes
```

Now I'll add the Istio Helm repo and search for packages:

```
helm repo add istio https://istio-release.storage.googleapis.com/charts

helm repo update

helm search repo istio
```

The first chart is the base, which sets up all the custom resource definitions (CRDs):

```
helm install istio-base istio/base -n istio-system --create-namespace
```

Next install the Istio control plane:

```
helm install istiod istio/istiod -n istio-system --wait
```

And finally the gateway, which will take care of routing traffic from outside the cluster into our Pods:

```
helm install istio-ingress istio/gateway -n istio-ingress --create-namespace  --wait

helm ls -A
```

## Deploy an App onto the Mesh

Istio can automatically inject the proxy container into any new Pods. You configure that by applying a label to the namespace(s) you want to use with Istio:

```
kubectl label namespace default istio-injection=enabled

kubectl describe namespace default
```

Now any Pods created in the `default` namespace will have Istio proxy containers injected.
We'll run a simple application to see what Istio does with it:

- [whoami.yaml](./whoami.yaml) - a Deployment and Service with no Istio configuration

```
kubectl apply -f demo1/whoami.yaml

kubectl get all
```

Check the containers in the Pod:

```
kubectl describe po -l app=whoami
```

> The istio-proxy container has a lot of configuration, and it's set with resource requests and limits

The Istio proxy is running, but currently it doesn't do much:

```
kubectl get svc whoami

curl http://localhost:8080
```

> This traffic goes from the LoadBalancer Service into the Pod