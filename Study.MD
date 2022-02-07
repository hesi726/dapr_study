## 第二章

https://www.cnblogs.com/chenyishi/p/15324032.html

### 注意事项，
dapr run --dapr-http-port 3501 --app-port 5001  --app-id frontend dotnet run 
运行时，可能会监听到不同的端口（例如 5155）,则无法从 3501 跳转到 5001 这个端口.

### 访问方法：
http://localhost:3501/v1.0/invoke/frontend/method/dapr/get2
dapr invoke --app-id frontend --verb "GET" --method dapr/get2

### 链路跟踪

自承载的方式下，Dapr默认启动了zipkin容器，可以通过以下链接查看

http://localhost:9411/zipkin/