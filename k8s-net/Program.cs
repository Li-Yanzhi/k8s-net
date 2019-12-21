using System;
using System.Threading;
using System.IO;
using k8s;
using k8s.Models;

namespace k8snet
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                try{
                    string svcToFind = "exservice";
                    var configFile = "/data/dubbo-env";
                    var retryInterval = 2000;
                    
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
                    var currentNs = string.IsNullOrEmpty(config.Namespace) ? "default" : config.Namespace;
                
                    Console.WriteLine($"Current Namespace:[{config.Namespace}], Host: [{config.Host}], User:[{config.Username}], Password: [{config.Password}], Context: [{config.CurrentContext}]");

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
                                    if(!string.IsNullOrEmpty(item.Status.LoadBalancer.Ingress[0].Hostname))
                                    {
                                        //Found IP, write to file
                                        var ip = item.Status.LoadBalancer.Ingress[0].Hostname;
                                        Console.WriteLine($"Get Service {svcToFind} current External-IP [{ip}] successfully!");
                                        found = true;

                                        Console.WriteLine($"Write Service IP to file {configFile} and exit");
                                        using (var writer = File.CreateText(configFile))
                                        {
                                            writer.WriteLine($"DUBBO={ip}"); //or .Write(), if you wish
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
                    Thread.Sleep(20 * 1000);
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
