# MotionResource (DC.Resource2)

基于HslCommunication的通用抽象运动控制模块。

## 📚 项目简介

DC.Resource2是一个通用的运动控制抽象模块，基于HslCommunication库开发，提供了统一的运动控制器接口，支持多种运动控制器（如DMC2210、DMC2410、固高、PLC等）的抽象和统一管理。

## 🎯 主要功能

- **统一运动控制接口**: 提供`IMotionController`接口，统一不同品牌运动控制器的操作
- **多控制器支持**: 
  - DMC2210运动控制器
  - DMC2410运动控制器
  - 固高（Googol）运动控制器
  - PLC运动控制器
- **地址管理**: 提供地址目录管理和验证功能
- **设备运动机制管理**: 设备运动机制（EMM）的数据库存储和管理
- **配置界面**: Windows Forms配置界面，方便参数配置和管理
- **数据库支持**: 使用Entity Framework和SQLite进行数据持久化

## 🛠️ 技术栈

- **语言**: C#
- **框架**: .NET Framework
- **通信库**: HslCommunication
- **ORM**: Entity Framework 6.4.4
- **数据库**: SQLite
- **日志**: Serilog
- **测试框架**: MSTest
- **Mock框架**: Moq

## 📁 项目结构

```
MotionResource/
├── DC.Resource2/                          # 主项目
│   ├── MontionControl/                   # 运动控制核心模块
│   │   ├── IMotionController.cs          # 运动控制器接口
│   │   ├── Dmc2210MotionController.cs    # DMC2210控制器实现
│   │   ├── Dmc2410MotionController.cs    # DMC2410控制器实现
│   │   ├── GoogolMotionController.cs     # 固高控制器实现
│   │   ├── PlcMotionController.cs        # PLC控制器实现
│   │   ├── MotionControllerFactory.cs    # 控制器工厂
│   │   ├── AggregatedMotionController.cs # 聚合运动控制器
│   │   ├── AddressCatalogDbRepository.cs # 地址目录仓储
│   │   ├── EquipmentMotionMechanismDbRepository.cs # EMM仓储
│   │   ├── MotionConfigForm.cs          # 配置界面
│   │   └── ...                          # 其他辅助类
│   └── Properties/                       # 项目属性
├── DC.Resource2.UnitTest/                # 单元测试项目
│   ├── AddressValidatorUnitTest.cs
│   ├── DbAddressCatalogUnitTest.cs
│   ├── DbMotionMechRepositoryUnitTest.cs
│   └── ...
├── DC.Resource2.IntegrationTest/         # 集成测试项目
│   ├── PLCForm.cs                        # PLC测试界面
│   └── ...
├── DC.Resource2.sln                      # 解决方案文件
└── packages/                             # NuGet包目录
```

## 🚀 快速开始

### 环境要求

- Visual Studio 2019或更高版本
- .NET Framework 4.x
- SQLite数据库

### 安装步骤

1. **克隆或下载项目**

2. **还原NuGet包**

   右键解决方案 -> 还原NuGet包

3. **配置数据库**

   项目使用SQLite数据库，首次运行会自动创建数据库文件。

4. **编译项目**

   使用Visual Studio打开 `DC.Resource2.sln`，编译解决方案。

### 基本使用

```csharp
// 创建运动控制器
var factory = new MotionControllerFactory();
var controller = factory.Create("DMC2210", connectionParams);

// 初始化
var result = controller.Initialize(pulsePerMM: 1000);

// 运动控制
if (result.Succeeded)
{
    // 绝对位置运动
    controller.Move(axisId: 0, nPulsePos: 10000, 
                   acc: 1000, dec: 1000, speed: 5000, 
                   motionType: MotionType.Absolute);
    
    // 等待运动完成
    var waitResult = controller.WaitDone(axisId: 0);
    
    // 回原点
    controller.GoHome(axisId: 0);
}

// 关闭连接
controller.Close();
```

## 📖 核心接口

### IMotionController

运动控制器核心接口，提供以下功能：

- `Initialize(float pulsePerMM)`: 初始化控制器
- `Move(...)`: 运动控制
- `GoHome(int axisId)`: 回原点
- `Stop(int axisId)`: 停止运动
- `WaitDone(int axisId)`: 等待运动完成
- `Read/Write`: 数据读写

### 支持的控制器类型

- **DMC2210**: 雷赛DMC2210运动控制器
- **DMC2410**: 雷赛DMC2410运动控制器
- **Googol**: 固高运动控制器
- **PLC**: 基于PLC的运动控制

## 🧪 测试

项目包含单元测试和集成测试：

- **单元测试**: 使用MSTest框架，测试各个组件的功能
- **集成测试**: 提供Windows Forms界面进行实际硬件测试

运行测试：
1. 打开Test Explorer（测试 -> 测试资源管理器）
2. 运行所有测试或选择特定测试

## 📝 配置说明

### 地址配置

通过`MotionConfigForm`界面可以配置：
- 地址目录（Address Catalog）
- 设备运动机制（Equipment Motion Mechanism）
- 功能码映射

### 数据库迁移

项目使用Entity Framework Code First，首次运行会自动创建数据库表结构。

## 🔧 扩展开发

### 添加新的运动控制器

1. 实现`IMotionController`接口
2. 在`MotionControllerFactory`中注册新控制器
3. 添加相应的配置和测试

## 📄 许可证

详见 [LICENSE](LICENSE) 文件

## 👤 作者

AMEZING77

## 📅 创建时间

2024年

## 🔗 相关资源

- [HslCommunication文档](https://www.cnblogs.com/dathlin/p/7703805.html)
- [Entity Framework文档](https://docs.microsoft.com/ef/)
