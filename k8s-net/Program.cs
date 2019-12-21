using System;
using System.Threading;
using System.IO;
using k8s;
using k8s.Models;

namespace k8snet
{
    class Program
    {
        private static string NamespaceFilePath = Path.Combine(new string[] {
                "/", "var", "run", "secrets", "kubernetes.io", "serviceaccount", "namespace"
            });

        static void Main(string[] args)
        {
            var exRetryInterval = 30 * 1000;
            while(true)
            {
                try{
                    string svcToFind = "exservice";
                    var configFile = "/data/dubbo-env";
                    var retryInterval = 5000;
                    
                    Console.WriteLine($"Container to resolve Server external IP");
                    if(args.Length < 1)
                    {
                        Console.WriteLine($"Cannot find the Service Name in parameters.");
                        //return;
                    }
                    else
                    {
                        svcToFind = args[0];
                    }
                    //var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
                    var config = KubernetesClientConfiguration.InClusterConfig();
                    var ns = File.ReadAllText(NamespaceFilePath);
                    
                    var currentNs = string.IsNullOrEmpty(ns) ? "default" : ns;
                
                    Console.WriteLine($"Current Namespace:{currentNs}");
                    Console.WriteLine($"Service to locate: {svcToFind}");
                    
                    IKubernetes client = new Kubernetes(config);
        
                    var found = false;
                    do 
                    {
                        Console.WriteLine($"Query Kubernetes Service spec...");

                        var svc = client.ListNamespacedService(currentNs);
                        
                        foreach (var item in svc.Items)
                        {
                            if(item.Metadata.Name.ToLower().Equals(svcToFind.ToLower()))
                            {
                                Console.WriteLine($"Found Service {svcToFind}");
                                if(item.Spec.Type.Equals("LoadBalancer"))
                                {
                                    var serviceIp = item.Status.LoadBalancer.Ingress[0].Ip;
                                    //var ip = item.Status.LoadBalancer.Ingress[0].Hostname;
                                    if(!string.IsNullOrEmpty(serviceIp))
                                    {
                                        //Found IP, write to file
                                        Console.WriteLine($"Get Service {svcToFind} current External-IP [{serviceIp}] successfully!");
                                        found = true;

                                        Console.WriteLine($"Write Service IP to file {configFile} and exit");
                                        using (var writer = File.CreateText(configFile))
                                        {
                                            writer.WriteLine($"DUBBO={serviceIp}"); //or .Write(), if you wish
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Cannot get Service {svcToFind} current External-IP, will retry after {retryInterval/1000} seconds...");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Service {svcToFind} type is not LoadBalancer, exit...");
                                    found = true;
                                }
                                break;
                            }
                        }
                        Thread.Sleep(retryInterval);
                    }while(!found);

                    return;
                }
                catch(Exception ex1)
                {
                    Console.WriteLine($"Error: {ex1.Message} \n\n {ex1.StackTrace}");
                    Thread.Sleep(exRetryInterval);
                }
            }


            // Console.WriteLine("press ctrl + c to stop watching");

            // var ctrlc = new ManualResetEventSlim(false);
            // Console.CancelKeyPress += (sender, eventArgs) => ctrlc.Set();
            // ctrlc.Wait();
            // var namespaces = client.ListNamespace();
            // foreach (var ns in namespaces.Items) {
            //     Console.WriteLine(ns.Metadata.Name);
            //     var list = client.ListNamespacedPod(ns.Metadata.Name);
            //     foreach (var item in list.Items)
            //     {
            //         Console.WriteLine(item.Metadata.Name);
            //     }
            // }

            // var podlistResp = client.ListNamespacedPodWithHttpMessagesAsync("default", watch: true);
            // using (podlistResp.Watch<V1Pod, V1PodList>((type, item) =>
            // {
            //     Console.WriteLine("==on watch event==");
            //     Console.WriteLine(type);
            //     Console.WriteLine(item.Metadata.Name);
            //     Console.WriteLine("==on watch event==");
            // }))
            // {
            //     Console.WriteLine("press ctrl + c to stop watching");

            //     var ctrlc = new ManualResetEventSlim(false);
            //     Console.CancelKeyPress += (sender, eventArgs) => ctrlc.Set();
            //     ctrlc.Wait();
            // }
        }
    }
}
