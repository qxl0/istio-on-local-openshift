# Managing Apps on Kubernetes with Istio (OpenShift Local)

This repository is a hands-on lab project for validating **core Istio/service mesh capabilities** on **OpenShift Local (CRC)**.

It is organized as progressive demos that cover traffic management, resiliency, security, observability, and platform integration patterns using Bookinfo and related sample services.

---

## Goals

- Validate Istio traffic routing and release strategies
- Test resiliency patterns (timeouts, retries, circuit breakers)
- Verify mesh security (mTLS, JWT authn, authz policies)
- Validate observability (tracing, access logs, EFK/Kibana)
- Run end-to-end tests on OpenShift Local

---

## Repository Layout

- `02/` Basic service mesh onboarding and first routing demos
- `03/` Traffic management patterns (blue/green, canary, fault handling)
- `04/` Security-focused demos (mTLS, authorization, JWT)
- `05/` OpenShift Local-focused module (`m5`) with tracing/logging scenarios
- `06/` AKS/OpenShift Local integration and platform-oriented demos
- `istio.yaml` Top-level mesh/operator-related configuration artifact

Each module contains its own demo-level README files and manifests.

---

## Prerequisites

- OpenShift Local (CRC) running
- `oc` CLI configured and logged in
- OpenShift Service Mesh operators installed (as required by module)
- Sufficient cluster memory/CPU for Bookinfo + observability stack

Optional but useful:
- `jq` for JSON inspection
- Browser access to OpenShift routes (`*.apps-crc.testing`)

---

## Quick Start

1. Start CRC and log in:

   - `crc start`
   - `oc login -u kubeadmin -p <password> https://api.crc.testing:6443`

2. Choose a module and follow its demo README in order:

   - Start with `02/`, then `03/`, `04/`, `05/`, `06/`

3. Apply manifests from the relevant demo folder:

   - `oc apply -f <manifest>.yaml`

4. Generate traffic (for tracing/logging demos) using Fortio or demo instructions.

---

## OpenShift Local Notes

- Prefer **Route** resources for external access instead of LoadBalancer services.
- Some demo images/manifests may require OpenShift-specific SCC/permissions adjustments.
- If using logging/tracing demos, confirm all backing pods are healthy before validating UI behavior.

---

## What to Validate in This Repo

- **Routing correctness:** traffic reaches expected version/subset
- **Policy enforcement:** authn/authz deny/allow behavior is as intended
- **Resiliency behavior:** retries/timeouts/circuit breaker effects are observable
- **Observability integrity:** traces/logs/metrics are generated and queryable
- **Repeatability:** demos can be re-run cleanly after restart/reset

---

## Troubleshooting Checklist

- `oc get pods -A` for crash loops/pending workloads
- `oc get events -A --sort-by=.lastTimestamp` for scheduling/SCC issues
- Verify routes and use `http://` vs `https://` as configured
- Re-generate traffic before checking tracing/logging UIs
- Restart affected workload after config changes:
  - `oc rollout restart deploy/<name> -n <namespace>`

---

## License

Use according to your organization or course/lab guidance.
