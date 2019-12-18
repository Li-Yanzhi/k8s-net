﻿using System;
using System.Threading;
using k8s;
using k8s.Models;

namespace k8snet
{
    class Program
    {
        static void Main(string[] args)
        {
            string svcToFind = "exservice";
            Console.WriteLine($"Container to resolve Server external IP");
            if(args.Length < 1)
            {
                Console.WriteLine($"Cannot find the Service Name in parameters.");
                //return;
            }
            //var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var config = KubernetesClientConfiguration.InClusterConfig();
            var currentNs = config.Namespace;
        
            Console.WriteLine($"Current Namespace:{currentNs}");
            //Console.WriteLine($"Service to locate:{args[0]}");
            Console.WriteLine($"Service to locate: {svcToFind}");
            
            IKubernetes client = new Kubernetes(config);

            var svc = client.ListNamespacedService(currentNs);
            var found = false;
            do 
            {
                Console.WriteLine($"Query Service Spec...");
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
                                Console.WriteLine($"Get Service {svcToFind} current External-IP {item.Status.LoadBalancer.Ingress[0].Hostname} successfully!");
                                found = true;
                            }
                            else
                            {
                                Console.WriteLine($"Cannot get Service {svcToFind} current External-IP, retry after 2 seconds...");
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
                Thread.Sleep(2000);
            }while(!found);


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
