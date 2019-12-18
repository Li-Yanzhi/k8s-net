# Create Kubernetes Resources

kubectl get pods
kubectl get service
kubectl apply -f myapp.yaml
kubectl get pods
kubectl logs myapp-pod -c init-get-service-ip
kubectl apply -f exservice.yaml
kubectl get service
kubectl get service exservice -o json
kubectl logs myapp-pod