# FSLIB.NETWORK 类库

> 本库目前仅供操作 HTTP 资源

## 背景

开始在设计订票助手.NET的时候，我就在策划写一个比较易用的HTTP客户端库来完成底层的操作。由于.NET原生的HttpWebRequest比较复杂难用，而内置的WebClient先天不足。

.NET 4.5中新增了HttpClient，但可惜.NET4.0不支持XP，所以暂时只能放弃HttpClient了。在这种种背景之下，我花了很长的时间来打磨这个网络库。虽然起名叫Network，但是目前专注于HTTP库。

这个网络库在订票助手.NET中得到了广泛全面的使用，几乎所有对12306发出的请求都是由它发出的。在订票助手.NET中，最近四个月中，由它发出的请求过亿，其稳定性也是蛮有保证的嘛。

## 功能特点&运行需求

其实它是对HttpWebRequest/HttpWebResponse的包装，目的是为了用起来更简单明了。设计的时候就为了提供更高的可用性和扩展性。所以……它具备……如下的特性。

* 高综合处理能力：自动处理Cookies，自动跟踪引用页，自动GZIP压缩解压缩，自动编码识别……
* 自动的数据处理能力：理论上你想发的数据，不用转换丢给它，它都能给你发出去；理论上你想收的对象，类型丢给它， 它都能给你弄回来……
* 高健壮性：如果不是特殊情况，坚决不抛异常让你去catch。相反的是，它用状态来向你表示结果是否正确
* 高处理能力：完全多线程处理，支持同步、异步、任务模式，异步时甚至能自动处理同步线程上下文，想用.NET中的await？没问题
* 高扩展性：丰富的事件以及扩展性支持，你可以继承它来实现自己想做的事情。甚至都自带了抓包。。。。

由于编写时使用了大量的匿名类型和表达式，因此不能用于.NET3.5以下的平台。目前支持的平台为.NET3.5/4/4.5。其中，运行在3.5平台上时，个别特性无法使用。

## 项目说明

项目采用 .NET CORE 项目格式（`project.json`），故需要最低 **Visual Studio 2015 Update3** 方可编译。

如果您不是有刚需自行编译项目的话，一般推荐直接通过 [NUGET 包管理器安装 FSLIB.NETWOK 包](https://www.nuget.org/packages/network.fishlee.net/)。

## 其它资源

* [使用手册](https://blog.fishlee.net/docs/fslib-network-basic/)
* [问答社区](http://ask.fishlee.net/category-21)：请在此处提问
* [鱼的博客上的相关文章](https://blog.fishlee.net/tag/fslib-network/)
* [官网主页](http://www.fishlee.net/)
* [鱼的博客](https://blog.fishlee.net/)
* [鱼的微博](http:weibo.com/imcfish/)

## 授权

Apache License
