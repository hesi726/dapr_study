## 基本概念

### SideCar 

* Dapr中，每个服务，每个运行的服务都会运行一个Sidecar；
* Dapr的服务间通信，服务与各中间件的通信都通过Sidecar来通信，
* 我们的代码里需要调用Dapr提供访问Sidecar的API，即可完成服务间通信，状态存储，服务监控等功能
* SideCar  
  1. 一个独立程序，作为在Dapr中运行的NetCore程序的代理，通过 HTTP 或者 GPRC 去访问NetCore服务或者其他Dapr支持的组件(例如Redis)
  2. NetCore和其他NetCore（或者Dapr组件)只能通过 SideCar进行相互访问.

  例如,在 Dapr中运行如下程序,则
```cmd  
  dapr run --dapr-http-port 3511 --app-port 5000  --app-id backend dotnet  .\E2_BackEnd\bin\Debug\net6.0\E2_BackEnd.dll
```
此NetCore程序对应的SideCar程序将为
```
daprd --app-id backend  --app-port 5000  --dapr-http-port 3511 --dapr-grpc-port 1247 --config c:\Users\dai_wei\.dapr\config.yaml --app-protocal http --profile-port -1 --log-level info --app-max-concurrency -1 --placement-host-addr
```

### 七大核心组件

1. 服务调用
> 要使用 Dapr 来调用应用程序，请在任意 Dapr 实例上使用 invoke 这个API。 
> sidecar 编程模型鼓励每个应用程序与自己的 Dapr 实例对话。 Dapr 实例会相互发现并进行通信。

Dapr服务调用Api:  
http://localhost:3500/v1.0/invoke/backend/method/neworder 

backend 是 appid 的名称  
neworder 是 backend 下面中某一个控制器.  

2. 状态管理（默认使用Redis)
> Dapr的状态存储组件支持可插拔，可以不用改代码而随意替换


Dapr状态Api:
http://localhost:3500/v1.0/state/myStateStore/planet 获取 planet 所存放的状态值  

上面url中， myStateStore 是状态组件的名称（默认为 statestore) ，此组件定义在 
a. windows下, C:\Users\<username>\.dapr\components\statestore.yaml  
b. linux下， ~/.dapr/components/statestore.yaml 


```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
  - name: actorStateStore
    value: "true"
```
默认使用 redis （在安装dapr时，将默认下载并启动一个 redis的docker镜像)  

3. 订阅
* 发布 / 订阅模式 允许微服务使用消息相互通信。 
> 生产者或发布者 将消息发送至 主题（Topic） ，并且不知道接收消息的应用程序。
> 消费者 将订阅该主题并收到它的消息，并且不知道什么应用程序生产了这些消息
> 中间消息代理（intermediary message broker）__由dapr实现__ 负责将每条消息从输入频道复制到所有对此消息感兴趣的订阅者的输出频道。

* Dapr 中的发布/订阅 API 提供至少一次（at-least-once）的保证，并与各种消息代理和队列系统集成。
> 服务所使用的特定实现是可插拔的，并被配置为运行时的 Dapr Pub/Sub 组件。
> 即可以很方便的将默认的 Redis发布订阅改为 RabbitMq或者其他.

Dapr 发布Api:
http://localhost:3500/v1.0/publish/pubsub/order 向order 主题发布消息.

上面url中， pubsub 是发布订阅组件的名称（默认为 pubsub) ，此组件定义在 
a. windows下, C:\Users\<username>\.dapr\components\pubsub.yaml  
b. linux下， ~/.dapr/components/pubsub.yaml 

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub
spec:
  type: pubsub.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
```

默认使用 redis （在安装dapr时，将默认下载并启动一个 redis的docker镜像)  

4. 资源绑定??

5. Actors ??
actor 模式 阐述了 Actors 为最低级别的“计算单元”。
> 换句话说，您将代码写入独立单元 ( 称为actor) ，该单元接收消息并一次处理消息，而不进行任何类型的并行或线程处理。

6. 可观测性
Dapr 使用 Zipkin 协议进行分布式跟踪和指标收集。  

* 跟踪一个Api通过 dapr 所进行的链式调用
* 收集性能指标

Dapr 将 HTTP/GRPC Middleware 添加到 Dapr sidecar。   
Middleware 拦截所有 Dapr 和应用程序流量，并自动注入关联ID以跟踪分布式事务。  


7. Secrets

应用程序通常会通过使用专用的密钥存储来秘密存储敏感信息，如连接字符串、密钥和用于与数据库、服务和外部系统进行验证的令牌。    
让开发人员在任何地方更容易消耗应用程序密钥， Dapr 有一个专用的密钥构建块 API ，允许开发人员从一个密钥存储获得密钥。  

使用 Dapr 的密钥存储构建块通常涉及以下内容：
1. 设置一个特定的密钥存储解决方案的组件。
2. 在应用程序代码中使用 Dapr 密钥 API 获取密钥。  
3. 可选，在Dapr组件文件中引用密钥。

Dapr访问 Secrets访问Api:
http://localhost:3500/v1.0/secrets/vault/mysecret 获取 mysecret的值.

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


## 状态管理

### 简单的状态管理调用:  
```c# 
        [HttpGet]
        public async Task<ActionResult> GetAsync()
        {
            var result = await _daprClient.GetStateAsync<string>("statestore", "guid");
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync()
        {
            await _daprClient.SaveStateAsync<string>("statestore", "guid", Guid.NewGuid().ToString(), new StateOptions { Consistency = ConsistencyMode.Strong });
            return Ok("done");
        }
```

* statestore  是 dapr下默认的状态管理组件的名称， 默认使用 redis.  
> new StateOptions { Consistency = ConsistencyMode.Strong } new StateOptions { Consistency = ConsistencyMode.Eventual }   ?? 

* 键名称使用 appid||key 形式，
> 如上面例子中,键名称为 frontend||guid  形式. 

* 键类型为 hash （data, version), version每次写入时都将发生变化.

```txt
127.0.0.1:6379> hgetall frontend||guid
1) "data"
2) "\"25186f6a-45c8-4810-870c-63f482195d68\""
3) "version"
4) "3"
```

### 防止并发冲突的状态管理调用: 

```c#
 //通过tag防止并发冲突，保存一个值
        [HttpPost("withtag")]
        public async Task<ActionResult> PostWithTagAsync()
        {
            var (value, etag) = await _daprClient.GetStateAndETagAsync<string>("statestore", "guid");
            await _daprClient.TrySaveStateAsync<string>("statestore", "guid", Guid.NewGuid().ToString(), etag);
            return Ok("done");
        }

        //通过tag防止并发冲突，删除一个值
        [HttpDelete("withtag")]
        public async Task<ActionResult> DeleteWithTagAsync()
        {
            var (value, etag) = await _daprClient.GetStateAndETagAsync<string>("statestore", "guid");
            return Ok(await _daprClient.TryDeleteStateAsync("statestore", "guid", etag));
        }
```
上述例子中，同时获取到 键值和键版本，并在更新时欻如键版本，以避免在多线程同时操作时，发生冲突. 


### 通过url模板访问和设置状态
 
* 通过URL模板和 FromStateAttribute 创建 StateEntry实例，并自动通过 statestore组件填充StateEntry实例的属性
* 修改StateEntry实例属性(只能修改其 value 属性）将自动保存到 statestore组件中.

```c#
 // 从绑定获取一个值，健值name从路由模板获取
        [HttpGet("frombinding/{name}")]
        public async Task<ActionResult> GetFromBindingAsync([FromState("statestore", "name")] StateEntry<string> state)
        {
            return Ok(state.Value);
        }


        // 根据绑定获取并修改值，健值name从路由模板获取
        [HttpPost("withbinding/{name}")]
        public async Task<ActionResult> PostWithBindingAsync([FromState("statestore", "name")] StateEntry<string> state)
        {            
            state.Value = Guid.NewGuid().ToString();
            return Ok(await state.TrySaveAsync());
        }
```
get http://localhost:3501/v1.0/invoke/frontend/method/state/frombinding/myname 
post http://localhost:3501/v1.0/invoke/frontend/method/state/withbinding/myname


### 批量访问和删除

```c#

// 获取多个个值
        [HttpGet("list")]
        public async Task<ActionResult> GetListAsync()
        {
            var result = await _daprClient.GetBulkStateAsync("statestore", new List<string> { "guid" }, 10);
            return Ok(result);
        }

        // 删除多个个值
        [HttpDelete("list")]
        public async Task<ActionResult> DeleteListAsync()
        {
            var data = await _daprClient.GetBulkStateAsync("statestore", new List<string> { "guid" }, 10);
            var removeList = new List<BulkDeleteStateItem>();
            foreach (var item in data)
            {
                removeList.Add(new BulkDeleteStateItem(item.Key, item.ETag));
            }
            await _daprClient.DeleteBulkStateAsync("statestore", removeList);
            return Ok("done");
        }
```
_daprClient.GetBulkStateAsync("statestore", new List<string> { "guid" }, 10);  
返回了如下形式的数组，即键名称，键值，键版本都一并返回

```json
[
    {
        "key": "guid",
        "value": "\"25186f6a-45c8-4810-870c-63f482195d68\"",
        "eTag": "3"
    }
]
```

* 删除键值时在 BulkDeleteStateItem 传入了键版本,判断了版本的一致性.  

## 发布订阅

* 默认情况下，发布一个消息后，dapr只会推送给一个订阅;
* 可以声明式订阅 或者 编程订阅
* 发布订阅组件 和 应用app 无关.

###  声明式订阅

* 在 dapr的组件目录下声明一个 订阅组件,
* 可以将发布、订阅完全解耦. 

```yaml
apiVersion: dapr.io/v1alpha1
kind: Subscription
metadata:
  name: myevent-subscription
spec:
  topic: test_topic
  route: /TestSub
  pubsubname: pubsub
scopes:
- frontend
```
kind: Subscription -- 表示此组件为一个订阅组件
topic: test_topic   -- 订阅主题
route: /TestSub     -- 收到消息发布到的路由       
pubsubname: pubsub  -- 从哪一个组件订阅

- frontend          -- 收到消息后发布目的的appid


### 编程订阅

* 通过 TopicAttribute 指定需要订阅的发布组件和订阅主题
* 通过 Request.Body 读取dapr发送过来的消息
* 启用 CloudEvents 中间件
* 使用如下代码注册订阅的标注处理映射

```c#
app.UseCloudEvents();  // 启用 CloudEvents 中间件
app.UseEndpoints((endpoints =>
{
    endpoints.MapSubscribeHandler(); // 映射 TopicAttribute 的订阅处理
}));
```

订阅的处理代码  
```c#

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Topic("pubsub", "test_topic")]
        [HttpPost("sub")]
        public async Task<ActionResult> Post()
        {
            Stream stream = Request.Body;
            byte[] buffer = new byte[Request.ContentLength.Value];
            stream.Position = 0L;
            stream.ReadAsync(buffer, 0, buffer.Length);
            var content = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("-----------------------------" + content + "----------------------------");
            return Ok(content);
        }
```

### 发布消息

通过 PublishEventAsync 向指定的组件、和主题 发布消息.

```c#
[HttpGet("pub")]
        public async Task<ActionResult> PubAsync()
        {
            var data = new WeatherForecast();
            await _daprClient.PublishEventAsync<WeatherForecast>("pubsub", "test_topic", data);
            return Ok();
        }
```

* 发布后例子代码发布消息时，只会向一个订阅发布；
* 貌似将优先向编程式订阅发布。

通过下列2中方式可以发布测试消息：

http://localhost:3501/v1.0/invoke/frontend/method/TestPubSub/pub   
dapr publish --publish-app-id frontend --pubsub pubsub --topic test_topic --data '{"date":"0001-01-01T00:00:00","temperatureC":0,"temperatureF":32,"summary":null}'  

## Actor模式

* Actors 为最低级别的“计算单元” 

> Actors模式是一段需要单线程执行的代码块。  
> Actors模式为分布式应用中的单线程逻辑提供了一种更好的选择   

