using k8s;
using k8s.KubeConfigModels;
using System.Text.Json;

//var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
var config = KubernetesClientConfiguration.BuildConfigFromConfigObject(
    new K8SConfiguration
    {
        Clusters = new List<Cluster>
        {
            new Cluster
            {
                Name = "talos-cluster",
                ClusterEndpoint = new ClusterEndpoint
                {
                    Server = "https://192.168.1.42:6443",
                    CertificateAuthorityData =
                        "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSUJpekNDQVRDZ0F3SUJBZ0lSQUlRc3VpU0hZTS8ramFJaDFYWE1LcVl3Q2dZSUtvWkl6ajBFQXdJd0ZURVQKTUJFR0ExVUVDaE1LYTNWaVpYSnVaWFJsY3pBZUZ3MHlNekE0TVRNeE5qQTBNVE5hRncwek16QTRNVEF4TmpBMApNVE5hTUJVeEV6QVJCZ05WQkFvVENtdDFZbVZ5Ym1WMFpYTXdXVEFUQmdjcWhrak9QUUlCQmdncWhrak9QUU1CCkJ3TkNBQVNDd2YvdTNFM2R5RmlnZWc1Q21peGtSZllZME5ibzF5TzV3d2tUY2l3azRlcm5aM05DckFrOE5FU3YKRi9RUnRvUFdCMGdSZHBUcW9XR1dXK1ZvcWdQTm8yRXdYekFPQmdOVkhROEJBZjhFQkFNQ0FvUXdIUVlEVlIwbApCQll3RkFZSUt3WUJCUVVIQXdFR0NDc0dBUVVGQndNQ01BOEdBMVVkRXdFQi93UUZNQU1CQWY4d0hRWURWUjBPCkJCWUVGSm9JS0YreE5NNVBOTUlHb0NrTUNjMTVCUEE5TUFvR0NDcUdTTTQ5QkFNQ0Ewa0FNRVlDSVFDMnJPYkQKazZJLzB6WEE0SDRyMmZ2QzU5Vk5Ud1BjU2ZPZzVHNm5MNFltS0FJaEFMb0RIejZDUDRUU2xhSTdGUi90eWR1UApvZ1crL3dJdkxYQWJ0dk9rYTl3YwotLS0tLUVORCBDRVJUSUZJQ0FURS0tLS0tCg=="
                }
            }
        },
        Users = new List<User>
        {
            new User
            {
                Name = "admin@talos-cluster",
                UserCredentials = new UserCredentials
                {
                    ClientCertificateData =
                        "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSUJoRENDQVN1Z0F3SUJBZ0lSQU5pTXg2RTRNaUhzYzl0SzVtWlR3aTR3Q2dZSUtvWkl6ajBFQXdJd0ZURVQKTUJFR0ExVUVDaE1LYTNWaVpYSnVaWFJsY3pBZUZ3MHlNekE0TVRNeE5qSXhNVFJhRncweU5EQTRNVEl4TmpJeApNalJhTUNreEZ6QVZCZ05WQkFvVERuTjVjM1JsYlRwdFlYTjBaWEp6TVE0d0RBWURWUVFERXdWaFpHMXBiakJaCk1CTUdCeXFHU000OUFnRUdDQ3FHU000OUF3RUhBMElBQkh1bHlPL1ZtVFNzRmV6VVhZMUJjZzMvdkhmbEh1dU4KRVQwa0dENTdTZ3I3YTl2aCs2RGtPMFBBVHpvKytoVnZyRnlIeEFETnVEQ1VqeVlQMjZlNFd1dWpTREJHTUE0RwpBMVVkRHdFQi93UUVBd0lGb0RBVEJnTlZIU1VFRERBS0JnZ3JCZ0VGQlFjREFqQWZCZ05WSFNNRUdEQVdnQlNhCkNDaGZzVFRPVHpUQ0JxQXBEQW5OZVFUd1BUQUtCZ2dxaGtqT1BRUURBZ05IQURCRUFpQkY5RjVrUFliQmU0OWoKOVU3Ym9GQ0Y2UXQyR1BLbkV2M0grMHF2NmFlamJBSWdFOUVMRi9ZZjdqYTdwT1A0K2JYY1hadDlYblU0Um5EUwo5SEd2YUVUZ2VzND0KLS0tLS1FTkQgQ0VSVElGSUNBVEUtLS0tLQo=",
                    ClientKeyData =
                        "LS0tLS1CRUdJTiBFQyBQUklWQVRFIEtFWS0tLS0tCk1IY0NBUUVFSUF3U2NhOVBJcGJodk91TlZIcUx2ZmU5YTU4Zk05eE9YZ2ErQmRoUzlTTlRvQW9HQ0NxR1NNNDkKQXdFSG9VUURRZ0FFZTZYSTc5V1pOS3dWN05SZGpVRnlEZis4ZCtVZTY0MFJQU1FZUG50S0N2dHIyK0g3b09RNwpROEJQT2o3NkZXK3NYSWZFQU0yNE1KU1BKZy9icDdoYTZ3PT0KLS0tLS1FTkQgRUMgUFJJVkFURSBLRVktLS0tLQo="
                }
            }
        },
        Contexts = new List<Context>
        {
            new Context
            {
                Name = "admin@talos-cluster",
                ContextDetails = new ContextDetails
                {
                    Cluster = "talos-cluster",
                    Namespace = "default",
                    User = "admin@talos-cluster"
                }
            }
        },
        CurrentContext = "admin@talos-cluster"
    }
);
var client = new K8Cloud.Test.Kubernetes(config);

var nodes = await client.CoreV1.ListNodeAsync();
Console.WriteLine(
    JsonSerializer.Serialize(nodes, new JsonSerializerOptions { WriteIndented = true })
);
//foreach (var node in nodes.Items)
//{
//    Console.WriteLine(node.Metadata.Name);
//}
//
//var pods = await client.CoreV1.ListPodForAllNamespacesAsync();
//foreach (var pod in pods.Items)
//{
//    Console.WriteLine(pod.Metadata.Name);
//}
//
//var isHealth = await client.IsHealth();
//Console.WriteLine(isHealth);
