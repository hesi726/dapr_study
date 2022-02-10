## 基本概念

https://www.cnblogs.com/chenyishi/p/15324032.html

* __所有Dapr程序都可以通过任意一个SideCar统一访问.__
  > 例如，如下2个例子程序, backend运行于 3511端口，frontend运行于 3500端口,
  > dapr run --dapr-http-port 3511 --app-port 5000  --app-id backend dotnet  .\bin\Debug\net6.0\E2_BackEnd.dll
  > dapr run --dapr-http-port 3500 --app-port 5001  --app-id frontend dotnet  .\bin\Debug\net6.0\E2_FrontEnd.dll
  >

  * http://localhost:3500/v1.0/invoke/backend/method/WeatherForecast
    > 可以访问到监听在 3511端口的 backend 中的 WeatherForecast 接口.
    >
  * http://localhost:3511/v1.0/invoke/frontend/method/actor/paid/126
    > 可以访问到监听在 3500 端口的 frontend 中的 actor 控制器中的 paid action.
    >
* __Dapr默认使用3500端口__
  * 外部客户端使用 ActorProxy 或者 DaprClient时，默认使用 3500端口，
    * 因此，至少应该存在一个监听在 3500 端口的 Dapr程序
  * 程序运行通过Dapr运行时, 使用 ActorProxy 或者 DaprClient时，可以正常解析,不需要 3500端口.

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

backend 是 appid 的名称neworder 是 backend 下面中某一个控制器.

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

Dapr 将 HTTP/GRPC Middleware 添加到 Dapr sidecar。Middleware 拦截所有 Dapr 和应用程序流量，并自动注入关联ID以跟踪分布式事务。

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

_daprClient.GetBulkStateAsync("statestore", new List`<string>` { "guid" }, 10);
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

### 声明式订阅

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
route: /TestSub     -- 收到消息发布到的路由pubsubname: pubsub  -- 从哪一个组件订阅

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

## Actors 模式

* Actors 为最低级别的“计算单元”

> Actors模式是一段需要单线程执行的代码块。
> Actors模式为分布式应用中的单线程逻辑提供了一种更好的选择
> 每一个Actor都具有 Id 和 类型，按照 Id 和 类型进行单线程的分类处理.

* 运行原理：

1. Dapr启动 app 后，SideCar 创建 Actors 后, 获取 Actors 的配置信息
2. SideCar 将 Actors 配置信息发送到 PlacementService
3. PlacementService 按照 Actors的 Id 和 Actor类型分区，并将其广播到所有的 dapr实例

> 日志类似如下这样,
> time="2022-02-09T10:47:15.9126249+08:00" level=info msg="placement tables updated, version: 61" app_id=backend instance=D2103026daiwei scope=dapr.runtime.actor.internal.placement type=log ver=1.6.0
> time="2022-02-09T11:03:50.0994116+08:00" level=info msg="placement tables updated, version: 62" app_id=backend instance=D2103026daiwei scope=dapr.runtime.actor.internal.placement type=log ver=1.6.0

4. 另一个SideCar 创建相同Id 和 类型的 Actors 代理实例时并调用其方法时，将转发到之前已创建的 Actors 处理；

(`<span style='color:red;'>`存疑，和打印的日志不符,创建 Actors的实例后，另外一个项目未看到广播的消息,而只能在dapr启动时看到广播消息)

### Actors模式实例

* 如果 Actor需要持久保存数据，则必须修改 状态存储组件，添加如下配置项:
  * Actor中存在 IActorStateManager StateManager 属性，用于持久保存数据
  * 持久数据的Hash键名称 appid||ActorType||ActorId||Key, 例如，"frontend||OrderStatusActor||od139||od139"
    * 上述例子中使用了 ActorId 作为了 Key名称，故重复了 od139

```yaml
- name: actorStateStore
  value: "true"
```

```csharp
//  添加或者更新数据，初次添加时,将设置为 init; 再次更新时,将值变为 paid;
await StateManager.AddOrUpdateStateAsync(orderId, "init", (key, currentState) => "paid");
//  获取给定键的值
await StateManager.GetStateAsync<string>(orderId);
```

* 按照需要 添加 Dapr.Actors和Dapr.Actors.AspNetCore 包.

1. 因为是代理实例，因此需要有一个继承自 IActor接口的业务接口;

```csharp
    public interface IOrderStatusActor : IActor
    {
        Task<string> Paid();
        Task<string> GetStatus();
    }
```

2. 实现业务接口的Actor业务类, 必须继承自Actor类

* Actor的Id在创建此 Actor类实例时, 传入到 Actor类的 Id属性,其类型为 ActorId.
* Actor的分类字符串在此类的实例中不可以访问,而是在创建代理时传入.

```csharp
public class OrderStatusActor : Actor, IOrderStatusActor
{
        public OrderStatusActor(ActorHost host) : base(host)
        {       
        }
        public async Task<string> GetStatus()
        {
            var orderId = this.Id.GetId();
            return await StateManager.GetStateAsync<string>(orderId);
        }
        public async Task<string> Paid()
        {      
            var orderId = this.Id.GetId();
            await StateManager.AddOrUpdateStateAsync(orderId, "init", (key, currentState) => "paid");
            return orderId;
        }
}
```

3. 启用Actor注册Api.

* Dapr SideCar 调用应用程序来承载和与执行组件实例进行交互，需要在Startup的Configure中注册 Actor.

```csharp
services.AddActors(options =>
{
   options.Actors.RegisterActor<OrderStatusActor>();
    options.ActorIdleTimeout = TimeSpan.FromMinutes(1); 
    options.ActorScanInterval = TimeSpan.FromSeconds(30);
    options.DrainOngoingCallTimeout = TimeSpan.FromSeconds(60);
    options.DrainRebalancedActors = true;
    options.RemindersStoragePartitions = 7;   
});
```

* 需要 Endpoing以便 Sidecar 能够通过接口访问注册的所有Actor，获取 Actor信息并同步到 Placement Service.

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapActorsHandlers();
    // .......
}); 
```

4. 使用Actor业务接口的客户端通过代理创建Actor业务类的代理实例.

* 创建代理实例时,需要传入 ActorId 和 Actor分类, 以便是唯一实例.

```csharp
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("paid/{orderId}")]
        public async Task<ActionResult> PaidAsync(string orderId)
        {
            var actorId = new ActorId("myid-" + orderId);
            var proxy = ActorProxy.Create<IOrderStatusActor>(actorId, nameof(OrderStatusActor));
            var result = await proxy.Paid();
            return Ok(result);
        }
```

### 计时器和提醒器

每一个Actor 都可以设置 timer 和 reminder 定时执行某些行为.

* timer 和 reminder 和 Actor实例相关，

  * 即每个Actor实例都单独执行定时任务和提醒任务.
* timer (类型为 ActorTimer) 只作用于激活状态的Actor。

  * 一个Actor长期不被调用，其自己的空闲计时器会逐渐累积，到一定时间后(参考上面代码中的 ActorIdleTimeout参数) 会被Dapr销毁;
  * timer没法作用于已销毁的Actor。
  * 注意, `<span style='color:red;'>`实际例子中，只要定时器启动了,则 Actor就不会空闲,也即不会销毁
* reminder则可以作用于所有状态的Actor。

  * 主要方式是重置空闲计时器，使其处于活跃状态

#### 定时器

##### 通过 Url 调用某一个定时器的方法

POST/PUT http://localhost:3500/v1.0/actors/\<actorType\>/\<actorId\>/timers/\<name\>
Post参数
{
  "dueTime":"0h0m9s0ms",
  "period":"0h0m3s0ms"
}

~~无法调用~~
a. http://localhost:3511/v1.0/invoke/frontend/method/actor/paid/139

> 激活 od139这个 actor后,

b. http://localhost:3511/v1.0/actors/OrderStatusActor/od139/timers/od-od139

变更定时器的定时配置?变更成功，但是定时器执行异常;
post json
{
  "dueTime":"0h0m1s0ms",
  "period":"0h0m1s0ms"
}
报错: "message": "error creating actor timer: can't create timer for actor OrderStatusActor||od139: actor not activated"

##### 通过代码创建定时器的方法.

1. 使用 Actor类 RegisterTimerAsync 异步方法创建定时器， 返回 Task\<ActorTimer\> 实例

> ActorTimer 类中有 ActorType 和 ActorId 属性.
> 此方法参数说明：
> a. 计时器的名称
> b. 触发计时器时要调用的回调方法的名称
> c. 要传递给回调方法的状态(字节数组 或者 null)
> d. 首次调用回调方法之前要等待的时间
> e. 回调方法调用之间的时间间隔。 可以指定 以 __TimeSpan.FromMilliseconds(-1)__ 禁用定期信号
>>> 禁用定期信号时，到达等待时间时,是会执行1次；只是禁止了定期执行;
>>> 注意，只能是 __TimeSpan.FromMilliseconds(-1)__, 不能是 TimeSpan.FromSeconds(-1) （将抛出异常）

2. 使用 Actor类 UnregisterTimerAsync 异步方法撤销定时器

```csharp
        ActorTimer timer;
        public async Task<string> Paid()
        {      
            var orderId = this.Id.GetId();
            Console.WriteLine("-----------" + orderId + "-----------------");
            await StateManager.AddOrUpdateStateAsync(orderId, "init", (key, currentState) => "paid");
            timer = await RegisterTimerAsync("od-" + orderId,
                 nameof(TimerCallbackAsync),
                 Encoding.UTF8.GetBytes(text),
                 TimeSpan.FromSeconds(5),
                 TimeSpan.FromSeconds(3));
            Console.WriteLine("-----------" + timer.ActorType + "-----------------" + timer.ActorId);
            return orderId;
        }

        public async Task TimerCallbackAsync(byte[] state)
        {
            var text = await this.GetStatus();
            _logger.LogInformation($" -------{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --  {this.Id.GetId()} -- Status: {text}  --------------");
            if (text == "paid")
            {
                await UnregisterTimerAsync(this.timer);
            }
        }
```

#### 提醒器 (Reminder)

* RegisterReminderAsync方法类似于 RegisterTimerAsync ，但不必显式指定回调方法。
* 实现 IRemindable.ReceiveReminderAsync 以处理触发的提醒。
  * ReceiveReminderAsync触发提醒时调用 方法。 它采用 4 个参数：
    * 提醒的名称。
    * 注册期间提供的用户状态。
    * 注册期间提供的延时执行时间。
    * 注册期间提供的调用周期。
* 使用 Actor类 UnregisterReminderAsync 异步方法撤销提醒器

```csharp
public class OrderStatusActor : Actor, IOrderStatusActor, IRemindable  
{
        IActorReminder actorReminder;
        public async Task<string> Paid()
        {  
            var orderId = this.Id.GetId();
            Console.WriteLine("-----------" + orderId + "-----------------");
            await StateManager.AddOrUpdateStateAsync(orderId, "init", (key, currentState) => "paid");
            this.actorReminder = await this.RegisterReminderAsync("reminder-" + orderId, null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(3));
            Console.WriteLine("-----------" + actorReminder.Name +  "-----------------" + actorReminder.State);
            return orderId;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var text = await this.GetStatus();
            _logger.LogInformation($" -------{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} --  {this.Id.GetId()} -- Status: {text}  --------------");
            if (text == "paid")
            {
                await UnregisterReminderAsync(this.actorReminder);
            }
        }
}
```

#### 定时器 和 提醒器的 区别

* app停止后重新启动时,定时器需要重新建立
* app停止后重新启动时，提醒器将会自动执行
  * 将会自动创建 Actor的实例，

例如如下代码，

```csharp
public async Task<string> Paid()
{      
            var orderId = this.Id.GetId();
            Console.WriteLine("-----------" + orderId + "-----------------");
            await StateManager.AddOrUpdateStateAsync(orderId, "init", (key, currentState) => "paid");
            this.actorReminder = await this.RegisterReminderAsync("reminder-" + orderId, null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(3));
            throw new Exception("测试提醒器是否会重启后自动创建Actor的实例并自动执行");
            return orderId;
}
```

去掉抛出异常代码并重启 app后，提醒器将会自动创建 OrderStatusActor 的实例（设置 Id 属性），并执行  ReceiveReminderAsync 异步方法。
并且重要的是，

1. __<span style='color:red;'>会补充执行上次时间到本次时间应该执行的次数。</span>__
2. __<span style='color:red;'>如果之前已经执行，则不会等待延时时间。</span>__

例如：
过 1 分钟重启 app, 间隔时间为 3秒时， 会立即执行 20次提醒器方法.

## 绑定
https://www.cnblogs.com/chenyishi/p/15343564.html

* 处理外部事件或调用外部接口的功能就是绑定
  * 输入绑定
    * 输入绑定是监听外部事件，触发业务逻辑
  * 输出绑定
    * 输出绑定是调用外部资源
* 绑定和发布订阅区别
  * 发布/订阅侧重于 Dapr services 之间的异步通信。
  * 绑定侧重于软件平台之间的系统互操作性。 
    * 在不同的应用程序、数据存储和微服务应用程序之外的服务之间交换信息。
* 通过绑定，可以避免多个应用程序之间的强的耦合;

### 工作原理
  
  * Dapr 资源绑定需要通过yaml文件定义绑定组件。 
    * 此 YAML 文件描述要与其绑定的资源类型。 配置后，你的服务可以接收来自资源的事件或触发事件。
  * 输入绑定
    * 通过外部资源的传入事件触发代码,即触发本应用中的某个方法.
   
    1. Dapr Sidecar读取绑定配置文件并订阅为外部资源。 
    2. 当外部资源触发时，在 Dapr sidecar中运行的绑定组件会选取它并触发一个事件
    3. Dapr sidecar根据配置文件调用指定的接口
    4. 处理事件后，服务将返回 HTTP 状态代码 200 OK
   
  * 输出绑定
    * 输出绑定使服务能够触发调用外部资源。 即向外发布消息.
     
    1. Dapr sidecar读取绑定配置文件，其中包含有关如何连接到外部资源的信息。
    2. 应用程序调用sidecar的 /v1.0/bindings/sms Dapr 终结点。 在这种情况下，它使用 HTTP POST 来调用 API。 还可以使用 gRPC。
    3. Dapr sidecar的绑定组件会调用外部消息系统来发送消息。 消息将包含 POST 请求中传递的负载。

  * 注意，组件名称(本例子中为 rabbitbinding )  应该和 webapi接口名称 (控制器类名称为 RabbitBindingController) 相同
    * 如果多个app中有相同的 名称为 RabbitBindingController 控制器类, 则都将进行绑定；
      * 目前 RabbitMq 使用轮询机制分发队列消息到各个绑定的 app
  
  #### 实例

1. 定义 RabbitBinding 组件.

格式: https://docs.dapr.io/reference/components-reference/supported-bindings/rabbitmq/

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: RabbitBinding
spec:
  type: bindings.rabbitmq
  version: v1
  metadata:
  - name: queueName   ## 队列名称
    value: queue1 
  - name: host        ## RabbitMq的连接字符串
    value: amqp://admin:123456@127.0.0.1:5672
  - name: durable    ## 只对输出绑定有用, Tells RabbitMQ to persist message in storage， 默认是 false
    value: true
  - name: deleteWhenUnused 
    value: false
  - name: ttlInSeconds  
    value: 60
  - name: prefetchCount
    value: 0
  - name: exclusive
    value: false
  - name: maxPriority
    value: 5
```

1. 处理代码

```csharp
 [Route("[controller]")]
    [ApiController]
    public class RabbitBindingController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetAsync([FromServices] DaprClient daprClient)
        {            
            await daprClient.InvokeBindingAsync("RabbitBinding", "create", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return Ok();
        }
        [HttpPost]
        public async Task<ActionResult> Post()
        {
           Stream stream = Request.Body;
           byte[] buffer = new byte[Request.ContentLength.Value];
           stream.Position = 0L;
           await stream.ReadAsync(buffer, 0, buffer.Length);
           string content = Encoding.UTF8.GetString(buffer);
           Console.WriteLine(".............binding............." + content);
           return Ok();
        }
    }
```

* daprClient.InvokeBindingAsync(string, string, string)
  * 调用一个输出绑定.
* 本例子中，
   http://localhost:3501/v1.0/invoke/frontend/method/RabbitBinding 创建输出绑定后, 会立即被本类中的输出绑定所处理;


## Secrets 敏感信息存储

支持的存储解决方案: 
https://docs.dapr.io/zh-hans/reference/components-reference/supported-secret-stores/

一个统一的地方存放那个所有的敏感信息，例如数据库连接字符串、密钥等等;


* 使用 Dapr 的密钥存储构建块通常涉及以下步骤：
1. 设置一个特定的密钥存储解决方案的组件。
2. 在应用程序代码中使用 Dapr Secrets API 获取密钥。
3. 在Dapr的Component文件中引用密钥

* 工作原理：
1. 服务A调用 Dapr Secrets API，提供要检索的Serects的名称和要查询的项名字。
> http://localhost:\<dapr-port>/v1.0/secrets/\<store-name>/\<name>?\<metadata>
>> \<dapr-port> 指定 Dapr sidecar 侦听的端口号。
>> \<store-name> 指定 Dapr Secrets存储的名称
>> \<name> 指定要检索的密钥的名称。
>> \<metadata> 提供Secrets的其他信息。 此段是可选的，每个Secrets存储的元数据属性不同。 有关元数据属性详细信息
2. Dapr sidecar 从Secrets存储中检索指定的机密。
3. Dapr sidecar 将Secrets信息返回给服务。

### 实际例子(使用本地json文件存放连接字符串)

* 连接字符串Json文件,文件位置： C:\Users\dai_wei\.dapr\components\secrets01.json
```json
{
    "RabbitMQConnectStr": "amqp://admin:123456@127.0.0.1:5672"
}
```
* Secrets定义文件: 
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: secrets01
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: C:\Users\username\.dapr\components\secrets01.json
  - name: nestedSeparator
    value: ":"
```   
* 访问 secrets的C#代码
```csharp
        [HttpGet]
        public async Task<ActionResult> GetAsync()
        {
            Dictionary<string, string> secrets = await _daprClient.GetSecretAsync("secrets01", "RabbitMQConnectStr");
            return Ok(secrets);
        }
``` 
> Api定义： 
`public abstract Task<Dictionary<string, string>> GetSecretAsync(string storeName, string key, IReadOnlyDictionary<string, string> metadata = null, CancellationToken cancellationToken = default);`

* 和 NetCore Configuration 集成

上面代码中，在控制器中访问已经太晚,因为连接字符串应该在 AddDbContext之前就应该已经配置好;  
需要提前到 Configution 中定义;

1. 首先引入nuget包 Dapr.Extensions.Configuration 
> dotnet add package nuget包Dapr.Extensions.Configuration   
2. 在配置 WebHost 的 WebAppblication时编写如下代码 （Net6形式的代码)
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:5001");
builder.WebHost.ConfigureAppConfiguration(config=> {
    var daprClient = new DaprClientBuilder().Build();
    var secretsDescriptors = new List<DaprSecretDescriptor> { new DaprSecretDescriptor("RabbitMQConnectStr") };
    config.AddDaprSecretStore("secrets01", secretsDescriptors, daprClient);      
});
``` 
3. __注意，上述代码中，会直接将 secrets01组件中的 RabbitMQConnectStr 键值对 存放到 Configuration中。__
> configuration["RabbitMQConnectStr"]  形式访问 RabbitMQConnectStr 的值.

* 其他组件引用 Secrets组件的值

很明显,有了 Secrets组件后,其他组件不应该自行定义数据库连接字符串等，而应该引用 secrets组件中的定义;

rabbitbinding.yaml 可以修改为如下形式，以便引用 secrets01中的 RabbitMQConnectStr  
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: RabbitBinding
spec:
  type: bindings.rabbitmq
  version: v1
  metadata:
  - name: queueName
    value: queue1
  - name: host
    secretKeyRef:
      name: RabbitMQConnectStr
      key: RabbitMQConnectStr
  - name: durable
    value: true
  - name: deleteWhenUnused
    value: false
  - name: ttlInSeconds
    value: 60
  - name: prefetchCount
    value: 0
  - name: exclusive
    value: false
  - name: maxPriority
    value: 5
auth:
  secretStore: secrets01
```

替换了如下定义，并添加了 auth 节点定义.
```yaml
- name: host 
  value: amqp://admin:123456@192.168.43.101:5672
```  

* secrets授权访问

跳过. 


## 服务监测

* 性能指标
* 链路跟踪
* 运行状况
* 日志记录

### Dapr 可观测性模块
1. 将服务监测与应用程序分离。
2. 自动捕获由 Dapr sidecar 和 Dapr 服务生成的流量。
3. 还公开性能指标、资源利用率和系统的运行状况。 
4. 监测测以开放标准格式发布，使信息能够馈入到选择的监视后端。
5. 当 Dapr 获取监控数据时，应用程序不需要知道如何实现可观测性。 应用程序无需引用库或实现自定义检测代码。 
6. Dapr可以使开发人员专注于构建业务逻辑，而不是监测。

### DaprSidecar:
1. 支持监测功能。 
   > 当服务间通信时，Dapr sidecar 会截获流量并提取跟踪、指标和日志记录信息。 
   > 监测以开放标准格式发布。 默认情况下，Dapr 支持 OpenTelemetry 和 Zipkin。

### Dapr收集器：
1. 可以将遥测数据发布到不同的后端监视工具。 这些工具可以查询分析 Dapr 遥测数据

### Zipkin服务跟踪

* dpar默认支持的分布式服务跟踪系统；
* Zipkin服务是在初始化dapr的时候，启动的一个docker容器
  > http://localhost:9411/zipkin/ 默认的 zipkin访问URL
  
c:\users\dai_wei\.dapr\config.yaml 中指定了 zipkin的配置信息
```yaml
apiVersion: dapr.io/v1alpha1
kind: Configuration
metadata:
  name: daprConfig
spec:
  tracing:
    samplingRate: "1"
    zipkin:
      endpointAddress: http://localhost:9411/api/v2/spans
```

### Dapr性能收集

* 默认下 Dapr SideCar 会开放 9000端口，向外暴露性能收集数据. 
* 性能收集数据默认为 Prometheus 格式
> 可以通过参数修改性能收集端口    --metrics-port 9091
> dapr run --dapr-http-port 3511 --app-port 5000 --app-id backend dotnet  .\BackEnd\bin\Debug\net5.0\BackEnd.dll --metrics-port 9090

http://10.20.14.6:9000  （不能使用 127.0.0.1)

返回说明
```txt
dapr_http_client_completed_count{app_id="frontend",method="GET",path="RabbitBinding",status="200"} 29  
dapr_http_client_completed_count{app_id="frontend",method="POST",path="RabbitBinding",status="200"} 15
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="1"} 0
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="2"} 14
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="3"} 20
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="4"} 23
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="5"} 25
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="6"} 26
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="8"} 26
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="10"} 26
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="13"} 26
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="16"} 26
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="10000"} 29
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="100000"} 29
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="GET",path="RabbitBinding",status="200",le="+Inf"} 29
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="POST",path="RabbitBinding",status="200",le="1"} 11
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="POST",path="RabbitBinding",status="200",le="1000"} 15
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="POST",path="RabbitBinding",status="200",le="100000"} 15
dapr_http_client_roundtrip_latency_bucket{app_id="frontend",method="POST",path="RabbitBinding",status="200",le="+Inf"} 15
```
通过 GET  请求 frontend RabbitBinding 29次,其中小于 2ms的响应为 14次.
通过 POST 请求 frontend RabbitBinding 15次,其中小于 1ms的响应为 11次.

### dapr 仪表盘（显示配置信息）

dapr dashboard 命令行启动仪表盘。

- 默认使用 8080端口，
- 显示有关 Dapr 应用程序、组件和配置的状态信息 

## 调试

1. 启动 daprd (即 SideBar)
  daprd -dapr-http-port 3501 -app-port 5001  -app-id frontend -placement-host-address localhost:6050 -components-path C:\\Users\\chesterychen\\.dapr\\components
  -app-port 应该是项目的HTTP 监听端口.

2. 在 Vs中启动项目.  


## K8S运行Dapr

<img src='https://img2020.cnblogs.com/blog/1033233/202109/1033233-20210929234430902-1712753654.png'>