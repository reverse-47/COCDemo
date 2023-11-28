using UnityEngine;
using Grpc.Net.Client;
using Cysharp.Net.Http;
using System;
using GrpcCOCDemo;

public class DemoClient : MonoBehaviour
{
    public string host = "http://192.168.50.222:20003"; //config
    private GrpcChannel channel;
    public COCDemo.COCDemoClient client;
    
    public DemoClient()
    {
        try 
        {
            // 创建一个自定义的HTTP处理程序（handler），并配置为只支持HTTP/2
            var handler = new YetAnotherHttpHandler(){ Http2Only = true };
            
            // 创建一个gRPC通道（channel）用于与gRPC服务器建立连接
            // 使用指定的服务器主机地址（host）和自定义的HTTP处理程序（handler）
            channel = GrpcChannel.ForAddress(host, new GrpcChannelOptions() { HttpHandler = handler });
            
            // 创建一个gRPC客户端实例，用于执行远程方法调用
            client = new COCDemo.COCDemoClient(channel);
            print("conected to server");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect to the server: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        channel.Dispose();
    }

    // public CheckFeasibleResponse CheckFeasible(CheckFeasibleRequest checkFeasibleRequest)
    // {
    //     return client.CheckFeasible(checkFeasibleRequest);
    // }

    // public GetSkillUsedResponse GetSkillUsed(GetSkillUsedRequest getSkillUsedRequest)
    // {
    //     return client.GetSkillUsed(getSkillUsedRequest);
    // }

    // public GetObjectReplyResponse GetObjectReply(GetObjectReplyRequest getObjectReplyRequest)
    // {
    //     return client.GetObjectReply(getObjectReplyRequest);
    // }

    // public GetObjectActionResponse GetObjectAction(GetObjectActionRequest getObjectActionRequest)
    // {
    //     return client.GetObjectAction(getObjectActionRequest);
    // }
}
