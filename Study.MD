## 第二章 Dapr服务和客户端

https://www.cnblogs.com/chenyishi/p/15324032.html

### 运行

1. 运行后端服务： 
dapr run --dapr-http-port 3511 --app-port 5000  --app-id backend dotnet  .\E2_BackEnd\bin\Debug\net6.0\E2_BackEnd.dll

通过Dapr CLI启动BackEnd，指定sidecar端口为3511，默认为3500，指定app-port是5000，与BackEnd默认端口保持一致。
通过下面2个URL可以访问上述的后端服务.

1.1 标准 NET6 的 URL 访问
http://localhost:5000/WeatherForecast

1.2 通过Dapr SideCar 访问
http://localhost:3501/v1.0/invoke/backend/method/WeatherForecast


2. 在  dapr中运行访问 DAPR后端服务器的客户端
dapr run --dapr-http-port 3501 --app-port 5001  --app-id frontend dotnet  .\E2_FrontEnd\bin\Debug\net6.0\E2_FrontEnd.dll

```c#
 // 通过HttpClient调用BackEnd
        [HttpGet]
        public async Task<ActionResult> GetAsync()
        {
            // Sidecar使用可插接式名称解析组件来解析服务BackEnd的地址。
            // 在自承载模式下，Dapr 使用 mdn 来查找它。
            // 在 Kubernetes 模式下运行时，Kubernetes DNS 服务将确定地址。
            using var httpClient = DaprClient.CreateInvokeHttpClient();
            var result = await httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>("http://backend/WeatherForecast");
            return Ok(result);
        }

        // 通过DaprClient调用BackEnd
        [HttpGet("get2")]
        public async Task<ActionResult> Get2Async()
        {
            using var daprClient = new DaprClientBuilder().Build();
            var result = await daprClient.InvokeMethodAsync<IEnumerable<WeatherForecast>>(HttpMethod.Get, "backend", "WeatherForecast");
            return Ok(result);
        }
```

* GetAsync API中通过DaprClient.CreateInvokeHttpClient()新建了HttpClient,通过GetAsync方法调用了backend服务中的WeatherForecastAPI。
> Sidecar使用可插接式名称解析组件来解析服务BackEnd的地址。  
> 在自承载模式下，Dapr 使用 mdn 来查找它。   
> 在 Kubernetes 模式下运行时，Kubernetes DNS 服务将确定地址。  

* dapr run --dapr-http-port 3501 --app-port 5001  --app-id frontend dotnet run 
> 使用 dotnet run 运行时，可能会监听到不同的端口（例如 5155）,则无法从 3501 跳转到 5001 这个端口.

2.1 客户端的访问：
http://localhost:3501/v1.0/invoke/frontend/method/dapr
http://localhost:3501/v1.0/invoke/frontend/method/dapr/get2
dapr invoke --app-id frontend --verb "GET" --method dapr/get2

## 链路跟踪

* 某一个接口的调用顺序。  

自承载的方式下，Dapr默认启动了zipkin容器，可以通过以下链接查看  
http://localhost:9411/zipkin/   某一个接口的调用顺序。


