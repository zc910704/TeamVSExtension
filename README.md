# Internal—team-dev-tool
team internal dev tool for visual studio 2019 extension


## Problems List:
+ [ ] 解决Package异步加载过慢的问题（2021年3月27日）  
+ [x] 解决COM对象调试问题（2021年3月28日）  
    - 使用托管兼容模式  
+ [ ] 关闭仅我的代码时Package不加载的问题（2021年3月28日）（会在服务器下载符号表）  
+ [x] 状态栏StatusPanel返回空，无法插入自定义控件（2021年3月31日）
    - 简单粗暴的在调试模式下找vs视图树里的状态栏即可。参考库[VS Window Manager](https://github.com/justcla/VSWindowManager/blob/master/VSWindowManager/Common/StatusBarButton.cs)

## Development Target:
- [x] 索引插件工程的所有文件
- [ ] 根据文件索引，提供工程内插件的信息VS服务，这些服务提供了该工程有哪些设备插件，对应哪些配置文件，哪些协议，协议有哪些和命令和参数的服务。  
- [ ] 协议命令参数高亮提示或常量代码自动生成  
- [ ] build时Dll自动拷贝依赖到项目中，自动中止程序  

## Support Feature List:
- 状态栏显示当前插件ID
- 通过状态栏切换插件
## ToDo: