using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace NBC.Asset
{
#if DEBUG

    public class DebugRemoteServer : MonoBehaviour
    {
        static HttpListener _httpListener;

        private readonly Queue<HttpListenerContext> _contexts = new Queue<HttpListenerContext>();

        void Start()
        {
            _httpListener = new HttpListener();
            //定义url及端口号，通常设置为配置文件
            _httpListener.Prefixes.Add("http://+:8080/");
            //启动监听器
            _httpListener.Start();
            _httpListener.BeginGetContext(Result, null);
            var ip = GetIP();
            Debug.Log($"调试服务端初始化完毕ip={ip}，正在等待调试客户端请求,时间：{DateTime.Now}");
        }

        private void Result(IAsyncResult ar)
        {
            //继续异步监听
            _httpListener.BeginGetContext(Result, null);
            var guid = Guid.NewGuid().ToString();
            Debug.Log($"接到新的请求:{guid},时间：{DateTime.Now}");
            //获得context对象
            var context = _httpListener.EndGetContext(ar);
            _contexts.Enqueue(context);
        }


        private string HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            response.StatusDescription = "200";
            response.StatusCode = 200;
            try
            {
                var info = Assets.GetDebugInfos();
                var json = JsonUtility.ToJson(info, true);
                return json;
            }
            catch (Exception e)
            {
                response.StatusDescription = "404";
                response.StatusCode = 404;
                return e.ToString();
            }
        }

        private void Update()
        {
            HandleContext();
        }

        private void HandleContext()
        {
            if (_contexts.Count < 1) return;
            var context = _contexts.Dequeue();
            var request = context.Request;
            var response = context.Response;
            // 如果是js的ajax请求，还可以设置跨域的ip地址与参数
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.ContentType = "text/plain;charset=UTF-8";
            context.Response.AddHeader("Content-type", "text/json"); //添加响应头信息
            context.Response.ContentEncoding = Encoding.UTF8;

            string returnObj = HandleRequest(request, response); //定义返回客户端的信息

            Debug.Log("返回内容=" + returnObj);
            var returnByteArr = Encoding.UTF8.GetBytes(returnObj); //设置客户端返回信息的编码
            try
            {
                using (var stream = response.OutputStream)
                {
                    //把处理信息返回到客户端
                    stream.Write(returnByteArr, 0, returnByteArr.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"远程调试异常：{ex}");
            }
        }

        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <returns></returns>
        private string GetIP()
        {
            try
            {
                var ips = Dns.GetHostAddresses(Dns.GetHostName());
                var localIp = ips.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                return localIp.ToString();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return string.Empty;
            }
        }
    }
#endif
}