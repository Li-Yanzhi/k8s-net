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
            //var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var config = KubernetesClientConfiguration.InClusterConfig();

            IKubernetes client = new Kubernetes(config);

            var svc = client.ListNamespacedService("default");
            foreach (var item in svc.Items)
            {
                Console.WriteLine("Name:" + item.Metadata.Name);
                Console.WriteLine("Type:" + item.Spec.Type);
                Console.WriteLine("Cluster-IP:" + item.Spec.ClusterIP);
                
                if(item.Spec.Type.Equals("LoadBalancer"))
                {
                    Console.WriteLine("Hostname:" + item.Status.LoadBalancer.Ingress[0].Hostname);
                    //Console.WriteLine("External-IP:" + item.Status.LoadBalancer.Ingress[0].Ip);
                }
                Console.WriteLine("----");
            }

            Console.WriteLine("press ctrl + c to stop watching");

            var ctrlc = new ManualResetEventSlim(false);
            Console.CancelKeyPress += (sender, eventArgs) => ctrlc.Set();
            ctrlc.Wait();
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
